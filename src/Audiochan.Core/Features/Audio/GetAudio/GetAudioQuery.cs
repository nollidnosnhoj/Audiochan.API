using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Mappings;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audio.GetAudio
{
    public record GetAudioQuery(long Id) : IRequest<Result<AudioViewModel>>
    {
        
    }

    public class GetAudioQueryHandler : IRequestHandler<GetAudioQuery, Result<AudioViewModel>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public GetAudioQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<Result<AudioViewModel>> Handle(GetAudioQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            
            var audio = await _dbContext.Audios
                .DefaultQueryable(currentUserId)
                .Where(x => x.Id == request.Id)
                .Select(MappingProfile.AudioMapToViewmodel(currentUserId))
                .SingleOrDefaultAsync(cancellationToken);

            return audio == null 
                ? Result<AudioViewModel>.Fail(ResultError.NotFound) 
                : Result<AudioViewModel>.Success(audio);
        }
    }
}