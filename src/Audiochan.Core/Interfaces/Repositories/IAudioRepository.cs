using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Search;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface IAudioRepository : IBaseRepository<Audio>
    {
        Task<TDto> GetAsync<TDto>(long id, CancellationToken cancellationToken = default);
        Task<TDto> RandomAsync<TDto>(CancellationToken cancellationToken = default);

        Task<PagedList<TDto>> ListAsync<TDto>(AudioListQueryRequest query,
            Expression<Func<Audio, bool>> whereExpression = null,
            CancellationToken cancellationToken = default);

        Task<PagedList<TDto>> SearchAsync<TDto>(SearchAudiosQuery query, CancellationToken cancellationToken = default);

        Task<PagedList<TDto>> FeedAsync<TDto>(string userId, int page, int size,
            CancellationToken cancellationToken = default);
    }
}