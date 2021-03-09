using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audios;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

namespace Audiochan.Core.Features.Search
{
    public record SearchAudiosQuery : AudioListQueryRequest
    {
        public string Q { get; init; }
        public string Tags { get; init; }
    }

    public class SearchAudiosQueryHandler : IRequestHandler<SearchAudiosQuery, PagedList<AudioViewModel>>
    {
        private readonly IAudioRepository _audioRepository;

        public SearchAudiosQueryHandler(IAudioRepository audioRepository)
        {
            _audioRepository = audioRepository;
        }

        public async Task<PagedList<AudioViewModel>> Handle(SearchAudiosQuery request, CancellationToken cancellationToken)
        {
            return await _audioRepository.SearchAsync<AudioViewModel>(request, cancellationToken);
        }
    }
}