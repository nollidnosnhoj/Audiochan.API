using System.Collections.Generic;
using Audiochan.Core.Common.Enums;

namespace Audiochan.Core.Common.Models
{
    public interface IResult<out TResponse>
    {
        TResponse Data { get; }
        bool IsSuccess { get; }
        ResultStatus? ErrorCode { get; }
        string Message { get; }
        Dictionary<string, string[]> Errors { get; }
    }

    public record Result<TResponse> : IResult<TResponse>
    {
        public TResponse Data { get; init; }
        public Dictionary<string, string[]> Errors { get; init; }
        public string Message { get; init; }
        public bool IsSuccess { get; init; }
        public ResultStatus? ErrorCode { get; init; }

        public static Result<TResponse> Fail(ResultStatus errorCode, string message = "", Dictionary<string, string[]> errors = null)
        {
            return new()
            {
                ErrorCode = errorCode,
                Message = GetDefaultMessage(errorCode, message),
                IsSuccess = false,
                Errors = errors
            };
        }

        public static Result<TResponse> Success(TResponse data)
        {
            return new()
            {
                IsSuccess = true,
                Data = data,
                Message = "Success"
            };
        }

        public static implicit operator bool(Result<TResponse> result)
        {
            return result.IsSuccess;
        }

        private static string GetDefaultMessage(ResultStatus errorCode, string message)
        {
            if (!string.IsNullOrWhiteSpace(message)) return message;

            return errorCode switch
            {
                ResultStatus.NotFound => "The requested resource was not found.",
                ResultStatus.Unauthorized => "You are not authorized access.",
                ResultStatus.Forbidden => "You are authorized, but forbidden access.",
                ResultStatus.UnprocessedEntity => "The request payload is invalid.",
                ResultStatus.BadRequest => "Unable to process request.",
                _ => "An unknown error has occurred."
            };
        }
    }
}