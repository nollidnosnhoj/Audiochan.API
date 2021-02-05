using ATL;
using Audiochan.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Audiochan.Infrastructure.Shared
{
    public class AudioMetadataService : IAudioMetadataService
    {
        public int GetDuration(IFormFile file)
        {
            var track = new Track(file.OpenReadStream(), file.ContentType);
            return track.Duration;
        }
    }
}