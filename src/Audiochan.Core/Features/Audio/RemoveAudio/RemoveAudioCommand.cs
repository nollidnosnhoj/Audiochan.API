using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Helpers;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audio.RemoveAudio
{
    public record RemoveAudioCommand(long Id) : IRequest<IResult<bool>>
    {
        
    }

    public class RemoveAudioCommandHandler : IRequestHandler<RemoveAudioCommand, IResult<bool>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;

        public RemoveAudioCommandHandler(IApplicationDbContext dbContext, IStorageService storageService, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
            _currentUserService = currentUserService;
        }
        
        public async Task<IResult<bool>> Handle(RemoveAudioCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            var audio = await _dbContext.Audios
                .SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (audio == null)
                return Result<bool>.Fail(ResultError.NotFound);

            if (audio.UserId != currentUserId)
                return Result<bool>.Fail(ResultError.Forbidden);
            
            _dbContext.Audios.Remove(audio);
            
            await _dbContext.SaveChangesAsync(cancellationToken);

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