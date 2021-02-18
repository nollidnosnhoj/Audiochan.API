using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Mappings;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Features.Audio.GetAudio;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audio.GetAudioFeed
{
    public record GetAudioFeedQuery : PaginationQuery, IRequest<PagedList<AudioViewModel>>
    {
        public string UserId { get; init; }
    }
    
    public class GetAudioFeedQueryHandler : IRequestHandler<GetAudioFeedQuery, PagedList<AudioViewModel>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetAudioFeedQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedList<AudioViewModel>> Handle(GetAudioFeedQuery request, CancellationToken cancellationToken)
        {
            var followedIds = await _dbContext.FollowedUsers
                .AsNoTracking()
                .Where(user => user.ObserverId == request.UserId)
                .Select(user => user.TargetId)
                .ToListAsync(cancellationToken);

            return await _dbContext.Audios
                .DefaultQueryable(request.UserId)
                .Where(a => followedIds.Contains(a.UserId))
                .Distinct()
                .Select(AudioMappings.Map(request.UserId))
                .OrderByDescending(a => a.Created)
                .Paginate(request, cancellationToken);
        }
    }
}