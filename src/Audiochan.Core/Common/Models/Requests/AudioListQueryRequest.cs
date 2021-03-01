using Audiochan.Core.Features.Audio.GetAudio;

namespace Audiochan.Core.Common.Models.Requests
{
    public abstract record AudioListQueryRequest : PaginationQueryRequest<AudioViewModel>
    {
        public string Genre { get; init; } = string.Empty;
        public string Sort { get; init; } = string.Empty;
    }
}