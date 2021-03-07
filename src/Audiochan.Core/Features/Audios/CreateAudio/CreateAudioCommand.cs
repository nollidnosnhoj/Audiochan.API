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
using Audiochan.Core.Services;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Audiochan.Core.Features.Audios.CreateAudio
{
    public record CreateAudioCommand : AudioCommandRequest, IRequest<Result<AudioViewModel>>
    {
        public Guid UploadId { get; init; }
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
        private readonly IApplicationDbContext _dbContext;
        private readonly TagService _tagService;
        private readonly GenreService _genreService;
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public CreateAudioCommandHandler(IApplicationDbContext dbContext,
            IStorageService storageService,
            ICurrentUserService currentUserService,
            IMapper mapper, 
            TagService tagService, GenreService genreService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _tagService = tagService;
            _genreService = genreService;
        }

        public async Task<Result<AudioViewModel>> Handle(CreateAudioCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            var audio = new Entities.Audio(request.UploadId, request.FileName, request.FileSize, request.Duration, currentUserId);

            audio.UpdateTitle(request.Title);
            audio.UpdateDescription(request.Description);
            audio.UpdatePublicStatus(request.IsPublic ?? true);

            if (!await CheckIfAudioBlobExists(audio, cancellationToken))
                return Result<AudioViewModel>.Fail(ResultError.BadRequest, "Cannot find audio in storage.");
            
            try
            {
                var genre = await _genreService.GetGenre(request.Genre, cancellationToken);
                audio.UpdateGenre(genre);
                
                var tags = request.Tags.Count > 0
                    ? await _tagService.CreateTags(request.Tags, cancellationToken)
                    : new List<Tag>();
                audio.UpdateTags(tags);

                await _dbContext.Audios.AddAsync(audio, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                var currentUser = await _dbContext.Users
                    .SingleOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
                
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

        private async Task<bool> CheckIfAudioBlobExists(Entities.Audio audio, CancellationToken cancellationToken = default)
        {
            return await _storageService.ExistsAsync(
                container: ContainerConstants.Audios,
                blobName: BlobHelpers.GetAudioBlobName(audio),
                cancellationToken);
        }
    }
}