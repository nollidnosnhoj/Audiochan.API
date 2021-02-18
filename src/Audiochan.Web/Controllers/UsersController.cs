﻿using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Features.Audio.GetAudio;
using Audiochan.Core.Features.Audio.GetAudioList;
using Audiochan.Core.Features.Favorites.Audios.GetFavoriteAudios;
using Audiochan.Core.Features.Followers.GetFollowers;
using Audiochan.Core.Features.Followers.GetFollowings;
using Audiochan.Core.Features.Users.GetProfile;
using Audiochan.Web.Extensions;
using Audiochan.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Audiochan.Web.Controllers
{
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{username}", Name="GetProfile")]
        [ProducesResponseType(typeof(ProfileViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Return user's profile.",
            OperationId = "GetProfile",
            Tags = new []{"users"}
        )]
        public async Task<IActionResult> GetUserProfile(string username, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProfileQuery(username), cancellationToken);

            return result.IsSuccess
                ? Ok(result.Data)
                : result.ReturnErrorResponse();
        }
        
        [HttpGet("{username}/audios", Name="GetUserAudios")]
        [ProducesResponseType(typeof(PagedList<AudioViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Return a list of the user's audios.",
            OperationId = "GetUserAudios",
            Tags = new []{"users"}
        )]
        public async Task<IActionResult> GetUserAudios(string username,
            PaginationQuery paginationQuery, CancellationToken cancellationToken)
        {
            var query = new GetAudioListQuery
            {
                Username = username,
                Page = paginationQuery.Page,
                Size = paginationQuery.Size
            };

            var list = await _mediator.Send(query, cancellationToken);

            return Ok(list);
        }

        [HttpGet("{username}/favorites/audios", Name="GetUserFavoriteAudios")]
        [ProducesResponseType(typeof(PagedList<AudioViewModel>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Return a list of the user's favorite audios.",
            OperationId = "GetUserFavoriteAudios",
            Tags = new []{"users"}
        )]
        public async Task<IActionResult> GetUserFavorites(string username, PaginationQuery paginationQuery,
            CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetFavoriteAudiosQuery
            {
                Username = username,
                Page = paginationQuery.Page,
                Size = paginationQuery.Size
            }, cancellationToken));
        }

        [HttpGet("{username}/followers", Name="GetUserFollowers")]
        [ProducesResponseType(typeof(PagedList<UserDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Return a list of the user's followers.",
            OperationId = "GetUserFollowers",
            Tags = new []{"users"}
        )]
        public async Task<IActionResult> GetFollowers(string username, [FromQuery] PaginationQuery query,
            CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetFollowersQuery
            {
                Username = username,
                Page = query.Page,
                Size = query.Size
            }, cancellationToken));
        }
        
        [HttpGet("{username}/followings", Name="GetUserFollowings")]
        [ProducesResponseType(typeof(PagedList<UserDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Return a list of the user's followings.",
            OperationId = "GetUserFollowings",
            Tags = new []{"users"}
        )]
        public async Task<IActionResult> GetFollowings(string username, [FromQuery] PaginationQuery query,
            CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetFollowingsQuery
            {
                Username = username,
                Page = query.Page,
                Size = query.Size
            }, cancellationToken));
        }
    }
}