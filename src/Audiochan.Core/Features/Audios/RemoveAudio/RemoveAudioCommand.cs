using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Helpers;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audios.RemoveAudio
{
    public record RemoveAudioCommand(long Id) : IRequest<IResult<bool>>
    {
        
    }

    public class RemoveAudioCommandHandler : IRequestHandler<RemoveAudioCommand, IResult<bool>>
    {
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAudioRepository _audioRepository;

        public RemoveAudioCommandHandler(IStorageService storageService, ICurrentUserService currentUserService, IAudioRepository audioRepository)
        {
            _storageService = storageService;
            _currentUserService = currentUserService;
            _audioRepository = audioRepository;
        }
        
        public async Task<IResult<bool>> Handle(RemoveAudioCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            
            var audio = await _audioRepository.SingleOrDefaultAsync(a => a.Id == request.Id, true, cancellationToken);

            if (audio == null)
                return Result<bool>.Fail(ResultError.NotFound);

            if (!audio.CanModify(currentUserId))
                return Result<bool>.Fail(ResultError.Forbidden);

            await _audioRepository.RemoveAsync(audio, cancellationToken);

            var tasks = new List<Task>
            {
                _storageService.RemoveAsync(ContainerConstants.Audios, BlobHelpers.GetAudioBlobName(audio), cancellationToken)
            };
            if (!string.IsNullOrEmpty(audio.Picture))
                tasks.Add(_storageService.RemoveAsync(audio.Picture, cancellationToken));
            await Task.WhenAll(tasks);
            return Result<bool>.Success(true);
        }
    }
}