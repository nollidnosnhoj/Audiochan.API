namespace Audiochan.Core.Common.Models
{
    /// <summary>
    /// Contains metadata from an audio file.
    /// </summary>
    public record AudioUploadResult(string StreamUrl, string Name, int Duration, long FileSize, string FileExt)
    {
    }
}