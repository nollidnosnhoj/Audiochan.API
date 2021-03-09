using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Entities;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface IGenreRepository : IBaseRepository<Genre>
    {
        Task<Genre> GetByInputAsync(string genre, CancellationToken cancellationToken = default);
    }
}