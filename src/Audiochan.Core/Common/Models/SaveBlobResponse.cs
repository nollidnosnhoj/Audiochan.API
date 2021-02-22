namespace Audiochan.Core.Common.Models
{
    public record SaveBlobResponse
    {
        public string Url { get; init; }
        public string Path { get; init; }
        public string ContentType { get; init; }
    }
}