using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Responses;
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
            var imageStream = await ProcessImage(data, cancellationToken);
            return await _storageService.SaveAsync(
                stream: imageStream,
                container: GetContainer(type),
                blobName: blobName,
                metadata: null,
                cancellationToken: cancellationToken);
        }

        public async Task RemoveImage(PictureType type, string blobName, CancellationToken cancellationToken = default)
        {
            await _storageService.RemoveAsync(GetContainer(type), blobName, cancellationToken);
        }

        private static async Task<Stream> ProcessImage(string imageData, CancellationToken cancellationToken = default)
        {
            // Parse the base64 data
            if (imageData.Contains("base64"))
                imageData = imageData.Split("base64")[1].Trim(',');
            
            var bytes = Convert.FromBase64String(imageData);
            
            // Resize the image to 500 x 500.
            using var imageContext = SixLabors.ImageSharp.Image.Load(bytes);
            var resizedImage = imageContext.Clone(x => x.Resize(500, 500));
            
            // Save the image context to JPEG
            var imageStream = new MemoryStream();
            await resizedImage.SaveAsJpegAsync(imageStream, cancellationToken);
            imageStream.Seek(0, SeekOrigin.Begin);
            
            return imageStream;
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