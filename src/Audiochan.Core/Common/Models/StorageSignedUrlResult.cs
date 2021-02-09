using System;

namespace Audiochan.Core.Common.Models
{
    public record StorageSignedUrlResult(Guid Id, string Url, string FileName = "") { }
}