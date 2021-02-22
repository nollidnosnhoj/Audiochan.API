using System;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;

namespace Audiochan.Core.Interfaces
{
    public interface IUploadService
    {
        (Guid UploadId, string Url) GetUploadUrl(string fileName);
    }
}