using Microsoft.AspNetCore.Http;

namespace Audiochan.Core.Interfaces
{
    public interface IAudioMetadataService
    {
        int GetDuration(IFormFile file);
    }
}