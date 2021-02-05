using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Audiochan.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IStorageService _storageService;

        public UploadController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpPost]
        public async Task<IActionResult> GetPresignedUrl([FromBody] GetPresignedUrlRequest request, 
            CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(request.FileName);
            // Generate ID for audio
            // TODO: Check for collision
            var id = Guid.NewGuid();
            var name = $"source{extension}";
            // Get presigned url to upload audio in the frontend
            var url = await _storageService.GetPresignedUrlAsync(
                container: ContainerConstants.Audios,
                blobName: Path.Combine(id.ToString(), name),
                cancellationToken);
            return Ok(new {Id = id, Url = url});
        }
    }
}