using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Search;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<TDto> GetAsync<TDto>(Expression<Func<User, bool>> expression,
            CancellationToken cancellationToken = default);
        Task<PagedList<TDto>> SearchAsync<TDto>(SearchUsersQuery query, CancellationToken cancellationToken = default);
    }
}