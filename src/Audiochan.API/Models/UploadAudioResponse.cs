using System;

namespace Audiochan.API.Models
{
    public record UploadAudioResponse(Guid UploadId, string Url)
    {
    }
}