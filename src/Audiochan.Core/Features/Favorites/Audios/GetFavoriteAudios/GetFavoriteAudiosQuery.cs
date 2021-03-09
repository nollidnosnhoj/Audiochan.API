using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using MediatR;

namespace Audiochan.Core.Features.Favorites.Audios.GetFavoriteAudios
{
    public record GetFavoriteAudiosQuery : PaginationQueryRequest<AudioViewModel>
    {
        public string Username { get; init; }
    }

    public class GetFavoriteAudiosQueryHandler : IRequestHandler<GetFavoriteAudiosQuery, PagedList<AudioViewModel>>
    {
        private readonly IFavoriteAudioRepository _favoriteAudioRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetFavoriteAudiosQueryHandler(IFavoriteAudioRepository favoriteAudioRepository, ICurrentUserService currentUserService)
        {
            _favoriteAudioRepository = favoriteAudioRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedList<AudioViewModel>> Handle(GetFavoriteAudiosQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            return await _favoriteAudioRepository.PagedListAsync<AudioViewModel>(
                request.Page,
                request.Size,
                fa => fa.User.UserName == request.Username.Trim().ToLower(),
                fa => fa.Created,
                true,
                false,
                new {currentUserId = currentUserId},
                cancellationToken);
        }
    }
}