using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Helpers;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Common.Options;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace Audiochan.Core.Features.Audios.CreateAudio
{
    public record CreateAudioCommand : AudioCommandRequest, IRequest<Result<AudioViewModel>>
    {
        public string UploadId { get; init; }
        public string FileName { get; init; }
        public long FileSize { get; init; }
        public int Duration { get; init; }
    }

    public class CreateAudioCommandValidator : AbstractValidator<CreateAudioCommand>
    {
        public CreateAudioCommandValidator(IOptions<AudiochanOptions> options)
        {
            var uploadOptions = options.Value.AudioUploadOptions;
            
            RuleFor(req => req.UploadId)
                .NotEmpty()
                .WithMessage("UploadId is required.");
            RuleFor(req => req.Duration)
                .NotEmpty()
                .WithMessage("Duration is required.");
            RuleFor(req => req.FileSize)
                .NotEmpty()
                .WithMessage("FileSize is required.");
            RuleFor(req => req.FileName)
                .NotEmpty()
                .WithMessage("Filename is required.")
                .Must(Path.HasExtension)
                .WithMessage("Filename must have a file extension.")
                .Must(fileName =>
                    uploadOptions.ContentTypes.Contains(Path.GetExtension(fileName).GetContentType()))
                .WithMessage("Filename is invalid.");

            Include(new AudioCommandValidator());
        }
    }

    public class CreateAudioCommandHandler : IRequestHandler<CreateAudioCommand, Result<AudioViewModel>>
    {
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IAudioRepository _audioRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly ITagRepository _tagRepository;

        public CreateAudioCommandHandler(IStorageService storageService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IAudioRepository audioRepository, 
            IUserRepository userRepository, 
            IGenreRepository genreRepository, ITagRepository tagRepository)
        {
            _storageService = storageService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _audioRepository = audioRepository;
            _userRepository = userRepository;
            _genreRepository = genreRepository;
            _tagRepository = tagRepository;
        }

        public async Task<Result<AudioViewModel>> Handle(CreateAudioCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            var audio = new Audio(request.UploadId, request.FileName, request.FileSize, request.Duration, currentUserId);

            audio.UpdateTitle(request.Title);
            audio.UpdateDescription(request.Description);
            audio.UpdatePublicStatus(request.IsPublic ?? true);

            if (!await CheckIfAudioBlobExists(audio, cancellationToken))
                return Result<AudioViewModel>.Fail(ResultError.BadRequest, "Cannot find audio in storage.");
            
            try
            {
                var genre = await _genreRepository.GetAsync(request.Genre, cancellationToken);
                audio.UpdateGenre(genre);
                
                var tags = request.Tags.Count > 0
                    ? await _tagRepository.InsertAsync(request.Tags, cancellationToken)
                    : new List<Tag>();
                audio.UpdateTags(tags);

                await _audioRepository.InsertAsync(audio, cancellationToken);

                var currentUser = await _userRepository
                    .SingleOrDefaultAsync(u => u.Id == currentUserId, true, cancellationToken);
                
                var viewModel = _mapper.Map<AudioViewModel>(audio) with
                {
                    User = new UserDto(currentUser.Id, currentUser.UserName, currentUser.Picture),
                    IsFavorited = audio.Favorited.Any(x => x.UserId == currentUserId)
                };
                
                return Result<AudioViewModel>.Success(viewModel);
            }
            catch (Exception)
            {
                await _storageService.RemoveAsync(ContainerConstants.Audios, BlobHelpers.GetAudioBlobName(audio), cancellationToken);
                throw; 
            }
        }

        private async Task<bool> CheckIfAudioBlobExists(Audio audio, CancellationToken cancellationToken = default)
        {
            return await _storageService.ExistsAsync(
                container: ContainerConstants.Audios,
                blobName: BlobHelpers.GetAudioBlobName(audio),
                cancellationToken);
        }
    }
}