using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Audiochan.Infrastructure.Image
{
    public class ImageService : IImageService
    {
        private readonly IStorageService _storageService;

        public ImageService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<SaveBlobResponse> UploadImage(string data, PictureType type, string blobName, 
            CancellationToken cancellationToken = default)
        {
            if (data.Contains("base64"))
            {
                data = data.Split("base64")[1].Trim(',');
            }
            var bytes = Convert.FromBase64String(data);
            using var imageContext = SixLabors.ImageSharp.Image.Load(bytes);
            var resizedImage = imageContext.Clone(x => x.Resize(500, 500));
            var imageStream = new MemoryStream();
            await resizedImage.SaveAsJpegAsync(imageStream, cancellationToken);
            imageStream.Seek(0, SeekOrigin.Begin);
            var saveRequest = new SaveBlobRequest(GetContainer(type), blobName, string.Empty);
            return await _storageService.SaveAsync(imageStream, saveRequest, cancellationToken);
        }

        public async Task RemoveImage(PictureType type, string blobName, CancellationToken cancellationToken = default)
        {
            await _storageService.RemoveAsync(GetContainer(type), blobName, cancellationToken);
        }

        private static string GetContainer(PictureType type)
        {
            return type switch
            {
                PictureType.Audio => Path.Combine(ContainerConstants.Pictures, ContainerConstants.Audios),
                PictureType.User => Path.Combine(ContainerConstants.Pictures, ContainerConstants.Users),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}