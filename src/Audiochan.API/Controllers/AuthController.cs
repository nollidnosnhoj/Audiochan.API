﻿using System.Threading;
using System.Threading.Tasks;
using Audiochan.API.Models;
using Audiochan.Core.Features.Auth.Login;
using Audiochan.Core.Features.Auth.Refresh;
using Audiochan.Core.Features.Auth.Register;
using Audiochan.Core.Features.Auth.Revoke;
using Audiochan.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Audiochan.API.Controllers
{
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login", Name="Login")]
        [ProducesResponseType(typeof(AuthResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Obtain access and refresh token using your login credentials.",
            OperationId = "Login",
            Tags = new []{ "auth" }
        )]
        public async Task<IActionResult> Login([FromBody] LoginCommand request, 
            CancellationToken cancellationToken)
        {
            var authResult = await _mediator.Send(request, cancellationToken);
            return authResult.IsSuccess 
                ? Ok(authResult.Data) 
                : authResult.ReturnErrorResponse();
        }
        
        [HttpPost("register", Name="CreateAccount")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation(
            Summary = "Create an account.",
            Description = "Once successful, you can use the login endpoint to obtain access and refresh tokens.",
            OperationId = "CreateAccount",
            Tags = new []{ "auth" }
        )]
        public async Task<IActionResult> Register([FromBody] RegisterCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return result.IsSuccess
                ? Ok()
                : result.ReturnErrorResponse();
        }

        [HttpPost("refresh", Name="RefreshAccessToken")]
        [ProducesResponseType(typeof(AuthResultViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Refresh access token using valid refresh token.",
            Description = "Once successful, you will also get a new refresh token, and the previous token will be invalid.",
            OperationId = "RefreshAccessToken",
            Tags = new []{"auth"}
        )]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var authResult = await _mediator.Send(request, cancellationToken);
            return authResult.IsSuccess 
                ? Ok(authResult.Data) 
                : authResult.ReturnErrorResponse();
        }

        [HttpPost("revoke", Name="RevokeRefreshToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Revoke a refresh token",
            OperationId = "RevokeRefreshToken",
            Tags = new []{"auth"}
        )]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenCommand request, 
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return result.IsSuccess
                ? Ok()
                : result.ReturnErrorResponse();
        }
    }
}