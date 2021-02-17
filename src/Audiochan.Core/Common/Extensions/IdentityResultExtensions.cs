using System.Collections.Generic;
using System.Linq;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Result;
using Microsoft.AspNetCore.Identity;

namespace Audiochan.Core.Common.Extensions
{
    public static class IdentityResultExtensions
    {
        public static IResult<bool> ToResult(this IdentityResult identityResult, string message = "")
        {
            if (identityResult.Succeeded) return Result<bool>.Success(true);

            return Result<bool>.Fail(
                ResultStatus.UnprocessedEntity,
                message,
                identityResult.FromIdentityToResultErrors());
        }
        
        public static IResult<TResponse> ToResult<TResponse>(this IdentityResult identityResult, TResponse data, string message = "")
        {
            if (identityResult.Succeeded) return Result<TResponse>.Success(data);
            
            return Result<TResponse>.Fail(
                ResultStatus.UnprocessedEntity,
                message,
                identityResult.FromIdentityToResultErrors());
        }

        private static Dictionary<string, string[]> FromIdentityToResultErrors(this IdentityResult identityResult)
        {
            return identityResult.Errors.ToDictionary(
                x => x.Code,
                x => new[] {x.Description});
        }
    }
}