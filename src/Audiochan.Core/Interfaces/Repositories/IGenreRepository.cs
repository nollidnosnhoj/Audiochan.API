using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Entities;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface IGenreRepository : IBaseRepository<Genre>
    {
        Task<Genre> GetAsync(string genre, CancellationToken cancellationToken = default);
        Task<List<TDto>> ListAsync<TDto>(ListGenresSort sort, CancellationToken cancellationToken = default);
    }
}