using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;

namespace Audiochan.Core.Interfaces
{
    public interface IUploadService
    {
        Task<StorageSignedUrlResult> GetPresignedUrl(GetPresignedUrlRequest request,
            CancellationToken cancellationToken = default);
    }
}