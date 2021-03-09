using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audios.GetAudioFeed
{
    public record GetAudioFeedQuery : PaginationQueryRequest<AudioViewModel>
    {
        public string UserId { get; init; }
    }
    
    public class GetAudioFeedQueryHandler : IRequestHandler<GetAudioFeedQuery, PagedList<AudioViewModel>>
    {
        private readonly IAudioRepository _audioRepository;

        public GetAudioFeedQueryHandler(IAudioRepository audioRepository)
        {
            _audioRepository = audioRepository;
        }

        public async Task<PagedList<AudioViewModel>> Handle(GetAudioFeedQuery request, CancellationToken cancellationToken)
        {
            return await _audioRepository.FeedAsync<AudioViewModel>(request.UserId, request.Page, request.Size,
                cancellationToken);
        }
    }
}