using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Followers.SetFollow
{
    public record SetFollowCommand(string UserId, string Username, bool IsFollowing) : IRequest<IResult<bool>>
    {
        
    }

    public class SetFollowCommandHandler : IRequestHandler<SetFollowCommand, IResult<bool>>
    {
        private readonly IUserRepository _userRepository;

        public SetFollowCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        public async Task<IResult<bool>> Handle(SetFollowCommand request, CancellationToken cancellationToken)
        {
            if (!await _userRepository.ExistsAsync(u => u.Id == request.UserId, cancellationToken))
                return Result<bool>.Fail(ResultError.Unauthorized);
            
            var target = await _userRepository
                .SingleOrDefaultAsync(u => u.UserName == request.Username.Trim().ToLower(), true, cancellationToken);

            if (target == null)
                return Result<bool>.Fail(ResultError.NotFound);

            if (target.Id == request.UserId)
                return Result<bool>.Fail(ResultError.Forbidden);

            var isFollowed = request.IsFollowing
                ? target.AddFollower(request.UserId)
                : target.RemoveFollower(request.UserId);

            await _userRepository.UpdateAsync(target, cancellationToken);

            return Result<bool>.Success(isFollowed);
        }
    }
}