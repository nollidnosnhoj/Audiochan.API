namespace Audiochan.Core.Common.Models
{
    public record GetPresignedUrlRequest
    {
        public string FileName { get; init; }
    }
}