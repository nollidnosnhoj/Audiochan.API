using System.Linq;
using Audiochan.Infrastructure.Storage.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace Audiochan.Infrastructure.Storage.Extensions
{
    public static class StorageExtensions
    {
        private static readonly FileExtensionContentTypeProvider Provider = new FileExtensionContentTypeProvider();
        
        public static string GetContentType(this string fileName)
        {
            if (!Provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        public static BlobPathInfo GetBlobPath(this string url, string storageUrl)
        {
            var path = url.Split(storageUrl).Last().TrimStart('/').Split('/');
            var name = path[^1];
            var container = string.Join('/', path[..^1]);
            return new BlobPathInfo(container, name);
        }
    }
}