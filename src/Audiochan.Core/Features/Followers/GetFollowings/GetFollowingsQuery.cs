using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Features.Followers.GetFollowers;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Followers.GetFollowings
{
    public record GetFollowingsQuery : PaginationQuery, IRequest<PagedList<UserDto>>
    {
        public string Username { get; init; }
    }
    
    public class GetFollowingsQueryHandler : IRequestHandler<GetFollowingsQuery, PagedList<UserDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IApplicationDbContext _dbContext;

        public GetFollowingsQueryHandler(ICurrentUserService currentUserService, IApplicationDbContext dbContext)
        {
            _currentUserService = currentUserService;
            _dbContext = dbContext;
        }
        
        public async Task<PagedList<UserDto>> Handle(GetFollowingsQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            return await _dbContext.FollowedUsers
                .AsNoTracking()
                .Include(u => u.Target)
                .Include(u => u.Observer)
                .ThenInclude(u => u.Followers)
                .Where(u => u.Observer.UserName == request.Username.Trim().ToLower())
                .Select(f => new UserDto(
                    f.TargetId,
                    f.Target.UserName,
                    f.Target.Picture,
                    f.Target.Followers.Any(x => x.ObserverId == currentUserId)))
                .Paginate(request, cancellationToken);
        }
    }
}