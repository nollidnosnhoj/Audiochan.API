using System;

namespace Audiochan.Core.Common.Models
{
    public record StorageSignedUrlResult(Guid UploadId, string Url, string FileName = "") { }
}