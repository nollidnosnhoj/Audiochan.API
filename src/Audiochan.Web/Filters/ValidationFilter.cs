﻿using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Web.Extensions;

namespace Audiochan.Web.Filters
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp =>
                        kvp.Value.Errors.Select(x => x.ErrorMessage).ToArray());
                var result = Result<bool>.Fail(ResultStatus.UnprocessedEntity, string.Empty, errors);
                context.Result = result.ReturnErrorResponse();
                return;
            }

            await next();
        }
    }
}
