using System.Collections.Generic;

namespace Audiochan.Core.Common.Models
{
    public record SaveBlobRequest
    {
        public string Container { get; init; }
        public string BlobName { get; init; }
        public string OriginalFileName { get; init; }
        public Dictionary<string, string> Metadata { get; init; } = new();
    }
}