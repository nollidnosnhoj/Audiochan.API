using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Result;
using Audiochan.Core.Features.Followers.Models;

namespace Audiochan.Core.Interfaces
{
    public interface IFollowerService
    {
        Task<PagedList<FollowUserViewModel>> GetUsersFollowers(string username, PaginationQuery paginationQuery,
            CancellationToken cancellationToken = default);

        Task<PagedList<FollowUserViewModel>> GetUsersFollowings(string username, PaginationQuery paginationQuery,
            CancellationToken cancellationToken = default);

        Task<bool> CheckFollowing(string userId, string username, CancellationToken cancellationToken = default);

        Task<IResult<bool>> Follow(string userId, string username, CancellationToken cancellationToken = default);
        
        Task<IResult<bool>> Unfollow(string userId, string username, CancellationToken cancellationToken = default);
    }
}