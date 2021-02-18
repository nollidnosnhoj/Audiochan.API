using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Followers.SetFollow
{
    public record SetFollowCommand(string UserId, string Username, bool IsFollowing) : IRequest<IResult<bool>>
    {
        
    }

    public class SetFollowCommandHandler : IRequestHandler<SetFollowCommand, IResult<bool>>
    {
        private readonly IApplicationDbContext _dbContext;

        public SetFollowCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<IResult<bool>> Handle(SetFollowCommand request, CancellationToken cancellationToken)
        {
            if (!await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == request.UserId, cancellationToken))
                return Result<bool>.Fail(ResultError.Unauthorized);
            
            var target = await _dbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserName == request.Username.Trim().ToLower(), cancellationToken);

            if (target == null)
                return Result<bool>.Fail(ResultError.NotFound);

            if (target.Id == request.UserId)
                return Result<bool>.Fail(ResultError.Forbidden);

            var followedUser = await _dbContext.FollowedUsers
                .SingleOrDefaultAsync(u => u.TargetId == target.Id 
                                           && u.ObserverId == request.UserId, cancellationToken);

            if (followedUser != null && !request.IsFollowing)
            {
                _dbContext.FollowedUsers.Remove(followedUser);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(false);
            }

            if (followedUser == null && request.IsFollowing)
            {
                followedUser = new FollowedUser
                {
                    TargetId = target.Id,
                    ObserverId = request.UserId,
                };

                await _dbContext.FollowedUsers.AddAsync(followedUser, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }

            return Result<bool>.Success(followedUser != null);
        }
    }
}