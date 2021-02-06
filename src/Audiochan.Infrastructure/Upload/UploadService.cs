using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;

namespace Audiochan.Infrastructure.Upload
{
    public class UploadService : IUploadService
    {
        private readonly IStorageService _storageService;

        public UploadService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<UploadResult> GetPresignedUrl(GetPresignedUrlRequest request, 
            CancellationToken cancellationToken = default)
        {
            var uploadId = Guid.NewGuid();
            // TODO: Check for UploadId Collision
            var presignedUrl = await _storageService.GetPresignedUrlAsync(
                container: ContainerConstants.Audios,
                blobName: Path.Combine(uploadId.ToString(), "source"),
                fileExtension: Path.GetExtension(request.FileName),
                cancellationToken: cancellationToken);

            return new UploadResult(uploadId, presignedUrl);
        }
    }
}