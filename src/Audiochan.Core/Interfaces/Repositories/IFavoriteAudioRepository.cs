using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface IFavoriteAudioRepository : IBaseRepository<FavoriteAudio>
    {
        Task<PagedList<TDto>> ListAsync<TDto>(string username, PaginationQueryRequest<TDto> paginationQuery,
            CancellationToken cancellationToken = default);
    }
}