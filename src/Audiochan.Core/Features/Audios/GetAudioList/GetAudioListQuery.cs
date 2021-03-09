using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces;
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
        private readonly ICurrentUserService _currentUserService;

        public GetAudioListQueryHandler(IAudioRepository audioRepository, ICurrentUserService currentUserService)
        {
            _audioRepository = audioRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedList<AudioViewModel>> Handle(GetAudioListQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            Expression<Func<Audio, bool>> wherePredicate = a => a.UserId == currentUserId || a.IsPublic;
            
            return await _audioRepository.ListAsync<AudioViewModel>(request, null, cancellationToken);
        }
    }
}