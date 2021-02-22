namespace Audiochan.Web.Models
{
    public record UploadAudioRequest
    {
        public string FileName { get; init; }
    }
}