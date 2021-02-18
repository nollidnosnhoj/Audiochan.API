using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Mappings;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Validators;
using Audiochan.Core.Features.Audio.GetAudio;
using Audiochan.Core.Features.Genres.GetGenre;
using Audiochan.Core.Features.Tags.CreateTags;
using Audiochan.Core.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audio.UpdateAudio
{
    public record UpdateAudioCommand : AudioCommand, IRequest<Result<AudioViewModel>>
    {
        [JsonIgnore] public long Id { get; init; }
    }

    public class UpdateAudioCommandValidator : AbstractValidator<UpdateAudioCommand>
    {
        public UpdateAudioCommandValidator()
        {
            Include(new AudioCommandValidator());
        }
    }

    public class UpdateAudioCommandHandler : IRequestHandler<UpdateAudioCommand, Result<AudioViewModel>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdateAudioCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService, IMediator mediator)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }
        
        public async Task<Result<AudioViewModel>> Handle(UpdateAudioCommand request, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var currentUserId = _currentUserService.GetUserId();

                var audio = await _dbContext.Audios
                    .Include(a => a.Favorited)
                    .Include(a => a.User)
                    .Include(a => a.Tags)
                    .Include(a => a.Genre)
                    .SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

                if (audio == null) 
                    return Result<AudioViewModel>.Fail(ResultStatus.NotFound);

                if (audio.UserId != currentUserId) 
                    return Result<AudioViewModel>.Fail(ResultStatus.Forbidden);

                audio.Title = !string.IsNullOrWhiteSpace(request.Title)
                    ? request.Title
                    : audio.Title;
                
                audio.Description = request.Description ?? audio.Description;
                
                audio.IsPublic = request.IsPublic ?? audio.IsPublic;
                
                audio.IsLoop = request.IsLoop ?? audio.IsLoop;

                if (!string.IsNullOrWhiteSpace(request.Genre) && (audio.Genre?.Slug ?? "") != request.Genre)
                {
                    var genre = await _mediator.Send(new GetGenreQuery(request.Genre), cancellationToken);

                    if (genre == null)
                        return Result<AudioViewModel>
                            .Fail(ResultStatus.BadRequest, "Genre does not exist.");

                    audio.Genre = genre!;
                }

                if (request.Tags.Count > 0)
                {
                    var newTags = await _mediator.Send(new CreateTagsCommand(request.Tags), cancellationToken);

                    foreach (var audioTag in audio.Tags)
                    {
                        if (newTags.All(t => t.Id != audioTag.Id))
                            audio.Tags.Remove(audioTag);
                    }

                    foreach (var newTag in newTags)
                    {
                        if (audio.Tags.All(t => t.Id != newTag.Id))
                            audio.Tags.Add(newTag);
                    }
                }

                _dbContext.Audios.Update(audio);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result<AudioViewModel>.Success(audio.MapToDetail(currentUserId));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}