using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Microsoft.AspNetCore.Http;

namespace Audiochan.Core.Interfaces
{
    public interface IAudioUploadService
    {
        Task<AudioUploadResult> Upload(IFormFile audioFile, string audioId, CancellationToken cancellationToken = default);
    }
}