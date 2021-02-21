using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Audiochan.Core.Features.Users.UpdateUser
{
    public record UpdateUserCommand : IRequest<IResult<bool>>
    {
        [JsonIgnore] public string UserId { get; init; }
        public string DisplayName { get; init; }
        public string About { get; init; }
        public string Website { get; init; }
    }
    
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, IResult<bool>>
    {
        private readonly UserManager<User> _userManager;

        public UpdateUserCommandHandler(UserManager<User> userManger)
        {
            _userManager = userManger;
        }
        
        public async Task<IResult<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return Result<bool>.Fail(ResultError.Unauthorized);
            
            // Rule: display name can only deviate by capitalization
            if (!string.IsNullOrWhiteSpace(request.DisplayName))
            {
                if (string.Equals(user.UserName.Trim(), request.DisplayName.Trim(),
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    user.DisplayName = request.DisplayName;
                }
            }
            
            user.About = request.About ?? user.About;
            user.Website = request.Website ?? user.Website;
            await _userManager.UpdateAsync(user);
            return Result<bool>.Success(true);
        }
    }
}