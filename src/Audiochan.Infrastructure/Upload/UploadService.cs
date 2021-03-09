﻿using System.Collections.Generic;
using System.IO;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Helpers;
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

        public (string UploadId, string Url) GetUploadUrl(string fileName)
        {
            var userId = _currentUserService.GetUserId();
            var uploadId = UploadHelpers.GenerateUploadId();
            var blobName = uploadId + Path.GetExtension(fileName);
            var metadata = new Dictionary<string, string> {{"UserId", userId}, {"OriginalFilename", fileName}};
            var uploadLink =
                _storageService.GetPresignedUrl(ContainerConstants.Audios, blobName, fileName, 5, metadata);
            return (uploadId, uploadLink);
        }
    }
}