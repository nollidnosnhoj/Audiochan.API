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
        public static IResult ToResult(this IdentityResult identityResult, string message = "")
        {
            if (identityResult.Succeeded) return Result.Success();

            return Result.Fail(
                ResultStatus.UnprocessedEntity,
                message,
                identityResult.FromIdentityToResultErrors());
        }
        
        public static IResult<T> ToResult<T>(this IdentityResult identityResult, T data, string message = "")
        {
            if (identityResult.Succeeded) return Result<T>.Success(data);
            
            return Result<T>.Fail(
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