using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces.Repositories;
using MediatR;

namespace Audiochan.Core.Features.Audios.GetAudioList
{
    public record GetAudioListQuery : AudioListQueryRequest
    {
    }

    public class GetAudioListQueryHandler : IRequestHandler<GetAudioListQuery, PagedList<AudioViewModel>>
    {
        private readonly IAudioRepository _audioRepository;

        public GetAudioListQueryHandler(IAudioRepository audioRepository)
        {
            _audioRepository = audioRepository;
        }

        public async Task<PagedList<AudioViewModel>> Handle(GetAudioListQuery request,
            CancellationToken cancellationToken)
        {
            return await _audioRepository.ListAsync<AudioViewModel>(request, null, cancellationToken);
        }
    }
}