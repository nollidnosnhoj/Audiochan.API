using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface IFollowerRepository : IBaseRepository<FollowedUser>
    {
        Task<PagedList<TDto>> ListAsync<TDto>(Expression<Func<FollowedUser, bool>> expression,
            PaginationQueryRequest<TDto> paginationQuery,
            CancellationToken cancellationToken = default);
    }
}