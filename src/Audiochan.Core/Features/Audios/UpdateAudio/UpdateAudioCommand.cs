using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Services;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audios.UpdateAudio
{
    public record UpdateAudioCommand : AudioCommandRequest, IRequest<Result<AudioViewModel>>
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
        private readonly TagService _tagService;
        private readonly GenreService _genreService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateAudioCommandHandler(IApplicationDbContext dbContext, 
            ICurrentUserService currentUserService, 
            IMapper mapper, 
            TagService tagService, 
            GenreService genreService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _tagService = tagService;
            _genreService = genreService;
        }
        
        public async Task<Result<AudioViewModel>> Handle(UpdateAudioCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            var audio = await _dbContext.Audios
                .Include(a => a.Favorited)
                .Include(a => a.User)
                .Include(a => a.Tags)
                .Include(a => a.Genre)
                .SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (audio == null) 
                return Result<AudioViewModel>.Fail(ResultError.NotFound);

            if (!audio.CanModify(currentUserId)) 
                return Result<AudioViewModel>.Fail(ResultError.Forbidden);

            if (!string.IsNullOrWhiteSpace(request.Genre) && (audio.Genre?.Slug ?? "") != request.Genre)
            {
                var genre = await _genreService.GetGenre(request.Genre, cancellationToken);

                if (genre == null)
                    return Result<AudioViewModel>.Fail(ResultError.BadRequest, "Genre does not exist.");

                audio.UpdateGenre(genre);
            }

            if (request.Tags.Count > 0)
            {
                var newTags = await _tagService.CreateTags(request.Tags, cancellationToken);

                audio.UpdateTags(newTags);
            }
            
            audio.UpdateTitle(request.Title);
            audio.UpdateDescription(request.Description);
            audio.UpdatePublicStatus(request.IsPublic);

            _dbContext.Audios.Update(audio);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            var viewModel = _mapper.Map<AudioViewModel>(audio) with
            {
                IsFavorited = audio.Favorited.Any(x => x.UserId == currentUserId)
            };
            
            return Result<AudioViewModel>.Success(viewModel);
        }
    }
}