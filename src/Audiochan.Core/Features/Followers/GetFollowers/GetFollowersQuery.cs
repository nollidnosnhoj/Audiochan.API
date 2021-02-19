using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Followers.GetFollowers
{
    public record GetFollowersQuery : PaginationQuery<UserDto>
    {
        public string Username { get; init; }
    }

    public class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, PagedList<UserDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IApplicationDbContext _dbContext;

        public GetFollowersQueryHandler(ICurrentUserService currentUserService, IApplicationDbContext dbContext)
        {
            _currentUserService = currentUserService;
            _dbContext = dbContext;
        }
        
        public async Task<PagedList<UserDto>> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            return await _dbContext.FollowedUsers
                .AsNoTracking()
                .Include(u => u.Target)
                .Include(u => u.Observer)
                .ThenInclude(u => u.Followers)
                .Where(u => u.Target.UserName == request.Username.Trim().ToLower())
                .Select(f => new UserDto(
                    f.ObserverId,
                    f.Observer.UserName,
                    f.Observer.Picture,
                    f.Observer.Followers.Any(x => x.ObserverId == currentUserId)))
                .PaginateAsync(request, cancellationToken);
        }
    }
}