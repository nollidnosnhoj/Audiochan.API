namespace Audiochan.Core.Common.Models
{
    public record SaveBlobResponse(
        string Url,
        string Path,
        string ContentType,
        string OriginalFileName)
    {
        
    }
}