using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;

namespace Audiochan.Infrastructure.Upload
{
    public class UploadService : IUploadService
    {
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;

        public UploadService(IStorageService storageService, ICurrentUserService currentUserService)
        {
            _storageService = storageService;
            _currentUserService = currentUserService;
        }

        public async Task<StorageSignedUrlResult> GetPresignedUrl(GetPresignedUrlRequest request, 
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.GetUserId();
            var uploadId = Guid.NewGuid();
            var blobName = Path.Combine(uploadId.ToString(), "source" + Path.GetExtension(request.FileName));
            var metaData = new Dictionary<string, string> {{"UserId", userId}};
            var blobRequest = new SaveBlobRequest(ContainerConstants.Audios, blobName, request.FileName,  metaData);
            var presignedUrl = _storageService.GetPresignedUrl(blobRequest);
            return await Task.FromResult(new StorageSignedUrlResult(uploadId, presignedUrl));
        }
    }
}