using System;
using System.IO;
using Audiochan.Core.Common.Constants;
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

        public GetUploadUrlResponse GetUploadUrl(GetUploadUrlRequest request)
        {
            var userId = _currentUserService.GetUserId();
            var uploadId = Guid.NewGuid();
            var blobName = uploadId + Path.GetExtension(request.FileName);
            var blobRequest = new SaveBlobRequest
            {
                Container = ContainerConstants.Audios,
                BlobName = blobName,
                OriginalFileName = request.FileName
            };
            blobRequest.Metadata.Add("UserId", userId);
            var uploadLink = _storageService.GetPresignedUrl(blobRequest);
            return new GetUploadUrlResponse {UploadId = uploadId, Url = uploadLink};
        }
    }
}