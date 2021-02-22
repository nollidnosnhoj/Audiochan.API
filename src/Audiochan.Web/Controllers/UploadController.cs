using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
using Audiochan.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Audiochan.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Get a temporary pre-signed AWS S3 Put link to upload audio.",
            OperationId = "GetPresignedUrl",
            Tags = new []{"upload"}
        )]
        public IActionResult GetUploadUrl([FromBody] UploadAudioRequest request)
        {
            var (uploadId, uploadLink) = _uploadService.GetUploadUrl(request.FileName);
            return new JsonResult(new UploadAudioResponse(uploadId, uploadLink));
        }
    }
}