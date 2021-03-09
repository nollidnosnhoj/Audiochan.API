using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audios.GetAudio;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audios.GetRandomAudio
{
    public record GetRandomAudioQuery : IRequest<Result<AudioViewModel>>
    {
        
    }

    public class GetRandomAudioQueryHandler : IRequestHandler<GetRandomAudioQuery, Result<AudioViewModel>>
    {
        private readonly IAudioRepository _audioRepository;

        public GetRandomAudioQueryHandler(IAudioRepository audioRepository)
        {
            _audioRepository = audioRepository;
        }
        
        public async Task<Result<AudioViewModel>> Handle(GetRandomAudioQuery request, CancellationToken cancellationToken)
        {
            var audio = await _audioRepository.RandomAsync<AudioViewModel>(cancellationToken);

            return audio == null 
                ? Result<AudioViewModel>.Fail(ResultError.NotFound) 
                : Result<AudioViewModel>.Success(audio);
        }
    }
}