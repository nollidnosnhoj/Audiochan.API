using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Interfaces.Repositories;
using MediatR;

namespace Audiochan.Core.Features.Followers.CheckIfFollowing
{
    public record CheckIfFollowingCommand(string UserId, string Username) : IRequest<bool>
    {
    }

    public class CheckIfFollowingCommandHandler : IRequestHandler<CheckIfFollowingCommand, bool>
    {
        private readonly IFollowerRepository _followerRepository;

        public CheckIfFollowingCommandHandler(IFollowerRepository followerRepository)
        {
            _followerRepository = followerRepository;
        }

        public async Task<bool> Handle(CheckIfFollowingCommand request, CancellationToken cancellationToken)
        {
            return await _followerRepository
                .ExistsAsync(u => u.ObserverId == request.UserId
                                  && u.Target.UserName == request.Username.Trim().ToLower(), cancellationToken);
        }
    }
}