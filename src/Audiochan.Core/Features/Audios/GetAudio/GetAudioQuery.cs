using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audios.GetAudio
{
    public record GetAudioQuery(long Id) : IRequest<Result<AudioViewModel>>
    {
        
    }

    public class GetAudioQueryHandler : IRequestHandler<GetAudioQuery, Result<AudioViewModel>>
    {
        private readonly IAudioRepository _audioRepository;

        public GetAudioQueryHandler(IAudioRepository audioRepository)
        {
            _audioRepository = audioRepository;
        }

        public async Task<Result<AudioViewModel>> Handle(GetAudioQuery request, CancellationToken cancellationToken)
        {
            var audio = await _audioRepository.GetAsync<AudioViewModel>(request.Id, cancellationToken);

            return audio == null 
                ? Result<AudioViewModel>.Fail(ResultError.NotFound) 
                : Result<AudioViewModel>.Success(audio);
        }
    }
}