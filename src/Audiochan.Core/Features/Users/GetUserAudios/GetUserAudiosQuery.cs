using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audios;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Features.Audios.GetAudioList;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

namespace Audiochan.Core.Features.Users.GetUserAudios
{
    public record GetUserAudiosQuery : AudioListQueryRequest
    {
        public string Username { get; init; }
    }
    
    public class GetUserAudiosQueryHandler : IRequestHandler<GetUserAudiosQuery, PagedList<AudioViewModel>>
    {

        private readonly IAudioRepository _audioRepository;

        public GetUserAudiosQueryHandler(IAudioRepository audioRepository)
        {
            _audioRepository = audioRepository;
        }

        public async Task<PagedList<AudioViewModel>> Handle(GetUserAudiosQuery request, CancellationToken cancellationToken)
        {
            return await _audioRepository.ListAsync<AudioViewModel>(request, a => a.User.UserName == request.Username.ToLower(), cancellationToken);
        }
    }
}