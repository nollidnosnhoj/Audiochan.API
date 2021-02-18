using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Options;
using Audiochan.Core.Common.Validators;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Genres.GetGenre;
using Audiochan.Core.Features.Tags.CreateTags;
using Audiochan.Core.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Audiochan.Core.Features.Audio.CreateAudio
{
    public record CreateAudioCommand : AudioCommand, IRequest<Result<CreateAudioResponse>>
    {
        public Guid UploadId { get; init; }
        public string FileName { get; init; }
        public int FileSize { get; init; }
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

    public class CreateAudioCommandHandler : IRequestHandler<CreateAudioCommand, Result<CreateAudioResponse>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public CreateAudioCommandHandler(IApplicationDbContext dbContext, IStorageService storageService, ICurrentUserService currentUserService, IMediator mediator)
        {
            _dbContext = dbContext;
            _storageService = storageService;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<Result<CreateAudioResponse>> Handle(CreateAudioCommand request, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var audio = new Entities.Audio
            {
                UploadId = request.UploadId,
                Title = !string.IsNullOrWhiteSpace(request.Title)
                    ? request.Title
                    : Path.GetFileNameWithoutExtension(request.FileName),
                Description = request.Description,
                Duration = request.Duration,
                FileExt = Path.GetExtension(request.FileName),
                IsPublic = request.IsPublic ?? true,
                IsLoop = request.IsLoop ?? false,
                FileSize = request.FileSize
            };
            
            var blobResponse = await _storageService.GetAsync(
                container: ContainerConstants.Audios,
                blobName: audio.GetBlobName(),
                cancellationToken);

            if (!blobResponse)
                return Result<CreateAudioResponse>.Fail(ResultStatus.BadRequest, "Cannot find audio in storage.");

            try
            {
                audio.User = await _dbContext.Users
                    .SingleOrDefaultAsync(u => u.Id == _currentUserService.GetUserId(), cancellationToken);
                audio.Genre = await _mediator.Send(new GetGenreQuery(request.Genre), cancellationToken);
                audio.Tags = request.Tags.Count > 0
                    ? await _mediator.Send(new CreateTagsCommand(request.Tags), cancellationToken)
                    : new List<Tag>();

                await _dbContext.Audios.AddAsync(audio, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                await transaction.CommitAsync(cancellationToken);
                return Result<CreateAudioResponse>.Success(new CreateAudioResponse(audio.Id));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _storageService.RemoveAsync(ContainerConstants.Audios, audio.GetBlobName(), cancellationToken);
                throw; 
            }
        }
    }
}