﻿using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Features.Audios.Models;
using Audiochan.Core.Interfaces;
using Audiochan.Web.Extensions;
using Audiochan.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Audiochan.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AudiosController : ControllerBase
    {
        private readonly IAudioService _audioService;

        public AudiosController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet(Name = "GetAudios")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedList<AudioViewModel>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Returns a list of audios.", OperationId = "GetAudios", Tags = new[] { "audios" })]
        public async Task<IActionResult> GetList([FromQuery] GetAudioListQuery query, 
            CancellationToken cancellationToken)
        {
            return Ok(await _audioService.GetList(query, cancellationToken));
        }

        [HttpGet("{audioId}", Name = "GetAudio")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AudioViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Return an audio by ID.", OperationId = "GetAudio", Tags = new [] { "audios" })]
        public async Task<IActionResult> Get(long audioId, CancellationToken cancellationToken)
        {
            var result = await _audioService.Get(audioId, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : result.ReturnErrorResponse();
        }

        [HttpGet("random", Name = "GetRandomAudio")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Return a random audio.", OperationId = "GetRandomAudio", Tags = new [] { "audios" })]
        public async Task<IActionResult> GetRandom(CancellationToken cancellationToken)
        {
            var result = await _audioService.GetRandom(cancellationToken);
            return result.IsSuccess 
                ? Ok(result.Data) 
                : result.ReturnErrorResponse();
        }

        [HttpPost(Name="UploadAudio")]
        [ProducesResponseType(typeof(AudioViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation(
            Summary = "Upload audio.",
            Description = "Requires authentication.",
            OperationId = "UploadAudio",
            Tags = new [] { "audios" })]
        public async Task<IActionResult> Upload(
            [FromForm] UploadAudioRequest request
            , CancellationToken cancellationToken)
        {
            var result = await _audioService.Create(request, cancellationToken);
            return result.IsSuccess 
                ? CreatedAtAction(nameof(Get), new { audioId = result.Data.Id }, result.Data)
                : result.ReturnErrorResponse();
        }

        [HttpPut("{audioId}", Name="UpdateAudio")]
        [ProducesResponseType(typeof(AudioViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation(
            Summary = "Update audio's details.",
            Description = "Requires authentication.",
            OperationId = "UpdateAudio",
            Tags = new [] { "audios" })]
        public async Task<IActionResult> Update(long audioId, [FromBody] UpdateAudioRequest request, 
            CancellationToken cancellationToken)
        {
            var result = await _audioService.Update(audioId, request, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : result.ReturnErrorResponse();
        }

        [HttpDelete("{audioId}", Name="DeleteAudio")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Remove audio.",
            Description = "Requires authentication.",
            OperationId = "DeleteAudio",
            Tags = new [] { "audios" })]
        public async Task<IActionResult> Destroy(long audioId, CancellationToken cancellationToken)
        {
            var result = await _audioService.Remove(audioId, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ReturnErrorResponse();
        }

        [HttpPatch("{audioId}/picture")]
        [SwaggerOperation(
            Summary = "Add Picture.",
            Description = "Requires authentication.",
            OperationId = "AddAudioPicture",
            Tags = new [] { "audios" })]
        public async Task<IActionResult> AddPicture(long audioId, [FromBody] AddPictureRequest request, 
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Data))
                return BadRequest();
            var result = await _audioService.AddPicture(audioId, request.Data, cancellationToken);
            return result.IsSuccess
                ? Ok(new {Image = result.Data})
                : result.ReturnErrorResponse();
        }
    }
}
