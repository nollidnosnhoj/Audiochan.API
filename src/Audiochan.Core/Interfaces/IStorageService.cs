using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;

namespace Audiochan.Core.Interfaces
{
    public interface IStorageService
    {
        string GetPresignedUrl(SaveBlobRequest request);
        Task RemoveAsync(string container, string blobName, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task<SaveBlobResponse> SaveAsync(Stream stream, SaveBlobRequest request, CancellationToken cancellationToken = default);
    }
}
