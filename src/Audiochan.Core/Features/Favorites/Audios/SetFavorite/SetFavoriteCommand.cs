using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Interfaces.Repositories;
using MediatR;

namespace Audiochan.Core.Features.Favorites.Audios.SetFavorite
{
    public record SetFavoriteCommand(string UserId, long AudioId, bool IsFavoriting) : IRequest<IResult<bool>>
    {
    }

    public class SetFavoriteCommandHandler : IRequestHandler<SetFavoriteCommand, IResult<bool>>
    {
        private readonly IAudioRepository _audioRepository;
        private readonly IUserRepository _userRepository;

        public SetFavoriteCommandHandler(IAudioRepository audioRepository, IUserRepository userRepository)
        {
            _audioRepository = audioRepository;
            _userRepository = userRepository;
        }


        public async Task<IResult<bool>> Handle(SetFavoriteCommand request, CancellationToken cancellationToken)
        {
            if (!await _userRepository.ExistsAsync(u => u.Id == request.UserId, cancellationToken))
                return Result<bool>.Fail(ResultError.Unauthorized);

            var audio = await _audioRepository.SingleOrDefaultAsync(a => a.Id == request.AudioId,
                true,
                cancellationToken);

            if (audio == null)
                return Result<bool>.Fail(ResultError.NotFound);

            if (audio.CanModify(request.UserId))
                return Result<bool>.Fail(ResultError.Forbidden);

            var favorited = request.IsFavoriting
                ? audio.AddFavorite(request.UserId)
                : audio.RemoveFavorite(request.UserId);

            await _audioRepository.UpdateAsync(audio, cancellationToken);

            return Result<bool>.Success(favorited);
        }
    }
}