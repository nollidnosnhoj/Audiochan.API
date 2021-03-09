using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Interfaces.Repositories;
using MediatR;

namespace Audiochan.Core.Features.Favorites.Audios.CheckIfFavoritedAudio
{
    public record CheckIfFavoritedAudioCommand(string UserId, long AudioId) : IRequest<bool>
    {
    }

    public class CheckIfFavoritedAudioCommandHandler : IRequestHandler<CheckIfFavoritedAudioCommand, bool>
    {
        private readonly IFavoriteAudioRepository _favoriteAudioRepository;

        public CheckIfFavoritedAudioCommandHandler(IFavoriteAudioRepository favoriteAudioRepository)
        {
            _favoriteAudioRepository = favoriteAudioRepository;
        }

        public async Task<bool> Handle(CheckIfFavoritedAudioCommand request, CancellationToken cancellationToken)
        {
            return await _favoriteAudioRepository.ExistsAsync(
                fa => fa.AudioId == request.AudioId && fa.UserId == request.UserId, cancellationToken);
        }
    }
}