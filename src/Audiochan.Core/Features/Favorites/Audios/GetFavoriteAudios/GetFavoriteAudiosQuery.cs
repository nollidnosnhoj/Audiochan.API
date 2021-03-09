using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Favorites.Audios.GetFavoriteAudios
{
    public record GetFavoriteAudiosQuery : PaginationQueryRequest<AudioViewModel>
    {
        public string Username { get; init; }
    }

    public class GetFavoriteAudiosQueryHandler : IRequestHandler<GetFavoriteAudiosQuery, PagedList<AudioViewModel>>
    {
        private readonly IFavoriteAudioRepository _favoriteAudioRepository;

        public GetFavoriteAudiosQueryHandler(IFavoriteAudioRepository favoriteAudioRepository)
        {
            _favoriteAudioRepository = favoriteAudioRepository;
        }
        
        public async Task<PagedList<AudioViewModel>> Handle(GetFavoriteAudiosQuery request, CancellationToken cancellationToken)
        {
            return await _favoriteAudioRepository.ListAsync(request.Username, request, cancellationToken);
        }
    }
}