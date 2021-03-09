using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Entities;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface ITagRepository : IBaseRepository<Tag>
    {
        Task<List<Tag>> InsertAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
    }
}