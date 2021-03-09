using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

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
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IAudioRepository _audioRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly ITagRepository _tagRepository;

        public UpdateAudioCommandHandler(ICurrentUserService currentUserService,
            IMapper mapper,
            IAudioRepository audioRepository,
            IGenreRepository genreRepository, ITagRepository tagRepository)
        {
            _currentUserService = currentUserService;
            _mapper = mapper;
            _audioRepository = audioRepository;
            _genreRepository = genreRepository;
            _tagRepository = tagRepository;
        }

        public async Task<Result<AudioViewModel>> Handle(UpdateAudioCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            var audio = await _audioRepository
                .SingleOrDefaultAsync(a => a.Id == request.Id, true, cancellationToken);

            if (audio == null)
                return Result<AudioViewModel>.Fail(ResultError.NotFound);

            if (!audio.CanModify(currentUserId))
                return Result<AudioViewModel>.Fail(ResultError.Forbidden);

            if (!string.IsNullOrWhiteSpace(request.Genre) && (audio.Genre?.Slug ?? "") != request.Genre)
            {
                var genre = await _genreRepository.GetByInputAsync(request.Genre, cancellationToken);

                if (genre == null)
                    return Result<AudioViewModel>.Fail(ResultError.BadRequest, "Genre does not exist.");

                audio.UpdateGenre(genre);
            }

            if (request.Tags.Count > 0)
            {
                var newTags = await _tagRepository.InsertAsync(request.Tags, cancellationToken);

                audio.UpdateTags(newTags);
            }

            audio.UpdateTitle(request.Title);
            audio.UpdateDescription(request.Description);
            audio.UpdatePublicStatus(request.IsPublic);

            await _audioRepository.UpdateAsync(audio, cancellationToken);

            var viewModel = _mapper.Map<AudioViewModel>(audio) with
            {
                IsFavorited = audio.Favorited.Any(x => x.UserId == currentUserId)
            };

            return Result<AudioViewModel>.Success(viewModel);
        }
    }
}