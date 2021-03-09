using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models.Responses;

namespace Audiochan.Core.Interfaces
{
    public interface IImageService
    {
        Task<SaveBlobResponse> UploadImage(string data, PictureType type, string blobName,
            CancellationToken cancellationToken = default);

        Task RemoveImage(PictureType type, string blobName, CancellationToken cancellationToken = default);
    }
}