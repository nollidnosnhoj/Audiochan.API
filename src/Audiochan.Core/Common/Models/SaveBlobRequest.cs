using System.Collections.Generic;

namespace Audiochan.Core.Common.Models
{
    public record SaveBlobRequest(
        string Container,
        string BlobName,
        string OriginalFileName,
        Dictionary<string, string> Metadata = null) { }
}