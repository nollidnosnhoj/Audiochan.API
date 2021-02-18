using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Users.GetCurrentUser
{
    public record GetCurrentUserQuery : IRequest<IResult<CurrentUserViewModel>>
    {
        
    }

    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, IResult<CurrentUserViewModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public GetCurrentUserQueryHandler(UserManager<User> userManager, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<IResult<CurrentUserViewModel>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            
            var user = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.Id == currentUserId)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                return Result<CurrentUserViewModel>.Fail(ResultError.Unauthorized);

            var roles = await _userManager.GetRolesAsync(user);

            return Result<CurrentUserViewModel>.Success(new CurrentUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName,
                Roles = roles
            });
        }
    }
}