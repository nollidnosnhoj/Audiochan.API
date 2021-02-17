using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Result;
using Audiochan.Core.Features.Audios.Models;

namespace Audiochan.Core.Interfaces
{
    public interface IFavoriteService
    {
        Task<PagedList<AudioViewModel>> GetUserFavorites(string username, PaginationQuery query,
            CancellationToken cancellationToken = default);
        Task<IResult<bool>> FavoriteAudio(string userId, long audioId, CancellationToken cancellationToken = default);
        Task<IResult<bool>> UnfavoriteAudio(string userId, long audioId, CancellationToken cancellationToken = default);
        Task<bool> CheckIfUserFavorited(string userId, long audioId, CancellationToken cancellationToken = default);
    }
}