using System;

namespace Audiochan.Web.Models
{
    public record UploadAudioResponse(Guid UploadId, string Url)
    {
    }
}