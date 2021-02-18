using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Mappings;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Users.GetProfile
{
    public record GetProfileQuery(string Username) : IRequest<IResult<ProfileViewModel>>
    {
        
    }

    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, IResult<ProfileViewModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public GetProfileQueryHandler(UserManager<User> userManager, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
        }
        
        public async Task<IResult<ProfileViewModel>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            
            var profile = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.Followers)
                .Include(u => u.Followings)
                .Include(u => u.Audios)
                .Where(u => u.UserName == request.Username.Trim().ToLower())
                .Select(UserMappings.Map(currentUserId))
                .SingleOrDefaultAsync(cancellationToken);

            return profile == null
                ? Result<ProfileViewModel>.Fail(ResultStatus.NotFound)
                : Result<ProfileViewModel>.Success(profile);
        }
    }
}