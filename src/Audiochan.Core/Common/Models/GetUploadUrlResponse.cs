using System;

namespace Audiochan.Core.Common.Models
{
    public record GetUploadUrlResponse
    {
        public Guid UploadId { get; init; }
        public string Url { get; init; }
        public string FileName { get; init; } = string.Empty;
    }
}