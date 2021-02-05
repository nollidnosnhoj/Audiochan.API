using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Audiochan.Infrastructure.Images
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IStorageService _storageService;

        public ImageUploadService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<BlobDto> Upload(PictureType type, IFormFile file, CancellationToken cancellationToken = default)
        {
            var name = Guid.NewGuid().ToString("N");
            using var image = await Image.LoadAsync(file.OpenReadStream());
            var imageOriginal = image.Clone(img =>
            {
                img.Resize(500, 500);
            });
            var imageStream = new MemoryStream();
            await imageOriginal.SaveAsync(imageStream, new JpegEncoder(), cancellationToken);
            imageStream.Seek(0, SeekOrigin.Begin);
            await _storageService.SaveBlobAsync(
                container: GetContainer(type),
                blobName: BlobWithExt(name), 
                stream: imageStream, 
                overwrite: true, 
                cancellationToken);
            var blob = await _storageService.GetBlobAsync(
                container: GetContainer(type), 
                blobName: BlobWithExt(name), 
                cancellationToken);

            return blob;
        }

        private static string BlobWithExt(string name) => name + ".jpg";

        private static string GetContainer(PictureType type) => type switch
        {
            PictureType.Audio => Path.Combine(ContainerConstants.Pictures, ContainerConstants.Audios),
            PictureType.User => Path.Combine(ContainerConstants.Pictures, ContainerConstants.Users),
            _ => ContainerConstants.Pictures
        };
    }
}