using System;
using System.IO;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Entities;

namespace Audiochan.Core.Common.Helpers
{
    public static class BlobHelpers
    {
        public static string GetAudioBlobName(Audio audio, bool includeContainer = false)
        {
            var name = audio.UploadId + audio.FileExt;
            return includeContainer
                ? Path.Combine(ContainerConstants.Audios, name)
                : name;
        }

        public static string GetPictureBlobPath(PictureType pictureType, string entityId)
        {
            var blobPath = Path.Combine(entityId, Guid.NewGuid().ToString("N"), "picture.jpg");
            return pictureType switch
            {
                PictureType.Audio => Path.Combine(ContainerConstants.Pictures, ContainerConstants.Audios, blobPath),
                PictureType.User => Path.Combine(ContainerConstants.Pictures, ContainerConstants.Users, blobPath),
                _ => throw new ArgumentOutOfRangeException(nameof(pictureType), pictureType, null)
            };
        }
    }
}