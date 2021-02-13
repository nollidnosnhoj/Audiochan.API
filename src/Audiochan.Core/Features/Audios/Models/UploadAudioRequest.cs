using System;
using Microsoft.AspNetCore.Http;

namespace Audiochan.Core.Features.Audios.Models
{
    public record UploadAudioRequest : UpdateAudioRequest
    {
        public Guid UploadId { get; init; }
        public string FileName { get; init; }
        public int FileSize { get; init; }
        public int Duration { get; init; }
    }
}