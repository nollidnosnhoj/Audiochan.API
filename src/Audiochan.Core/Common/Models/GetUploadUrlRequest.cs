namespace Audiochan.Core.Common.Models
{
    public record GetUploadUrlRequest
    {
        public string FileName { get; init; }
    }
}