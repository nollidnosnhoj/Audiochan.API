﻿using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Result;
using Audiochan.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Audiochan.Web.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ReturnErrorResponse<TResponse>(this IResult<TResponse> result)
        {
            var response = new ErrorViewModel(result.ToErrorCode(), result.Message, result.Errors);
            return new ObjectResult(response) {StatusCode = response.Code};
        }

        private static int ToErrorCode<TResponse>(this IResult<TResponse> result)
        {
            return result.ErrorCode switch
            {
                ResultStatus.NotFound => StatusCodes.Status404NotFound,
                ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
                ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                ResultStatus.UnprocessedEntity => StatusCodes.Status422UnprocessableEntity,
                ResultStatus.BadRequest => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}