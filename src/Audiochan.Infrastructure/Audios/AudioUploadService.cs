using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ATL;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Audiochan.Infrastructure.Audios
{
    public class AudioUploadService : IAudioUploadService
    {
        private readonly IStorageService _storageService;

        public AudioUploadService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<AudioUploadResult> Upload(IFormFile audioFile, string audioId, CancellationToken cancellationToken = default)
        {
            await using var audioStream = audioFile.OpenReadStream();
            var audioMetadata = new Track(audioStream, audioFile.ContentType);
            var blobName = $"{audioId}{Path.GetExtension(audioFile.FileName)}";
            await _storageService
                .SaveAsync(ContainerConstants.Audios, blobName, audioStream, false, cancellationToken);
            var blobDto = await _storageService
                .GetAsync(ContainerConstants.Audios, blobName, cancellationToken);

            return new AudioUploadResult(
                blobDto.Url,
                blobName,
                audioMetadata.Duration,
                blobDto.Size,
                Path.GetExtension(audioFile.FileName));
        }
    }
}