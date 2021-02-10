using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;

namespace Audiochan.Core.Interfaces
{
    public interface IUploadService
    {
        GetUploadUrlResponse GetUploadUrl(GetUploadUrlRequest request);
    }
}