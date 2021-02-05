using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Microsoft.AspNetCore.Http;

namespace Audiochan.Core.Interfaces
{
    public interface IImageUploadService
    {
        Task<BlobDto> Upload(PictureType type, IFormFile file,  CancellationToken cancellationToken = default);
    }
}