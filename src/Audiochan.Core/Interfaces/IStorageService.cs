using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;

namespace Audiochan.Core.Interfaces
{
    public interface IStorageService
    {
        
        Task RemoveAsync(string url, CancellationToken cancellationToken = default);
        Task SaveAsync(string container, string blobName, Stream stream, bool overwrite = true,
            CancellationToken cancellationToken = default);
        Task<BlobDto> GetAsync(string container, string blobName,
            CancellationToken cancellationToken = default);
    }
}
