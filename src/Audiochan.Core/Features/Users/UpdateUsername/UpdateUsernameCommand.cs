using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Users.UpdateUser;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Audiochan.Core.Features.Users.UpdateUsername
{
    public record UpdateUsernameCommand : IRequest<IResult<bool>>
    {
        [JsonIgnore] public string UserId { get; init; }
        public string NewUsername { get; init; }
    }

    public class UpdateUsernameCommandValidator : AbstractValidator<UpdateUsernameCommand>
    {
        public UpdateUsernameCommandValidator(IOptions<Audiochan.Core.Common.Options.IdentityOptions> options)
        {
            RuleFor(req => req.NewUsername).Username(options.Value);
        }
    }
    
    public class UpdateUsernameCommandHandler : IRequestHandler<UpdateUsernameCommand, IResult<bool>>
    {
        private readonly UserManager<User> _userManager;

        public UpdateUsernameCommandHandler(UserManager<User> userManger)
        {
            _userManager = userManger;
        }

        public async Task<IResult<bool>> Handle(UpdateUsernameCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return Result<bool>.Fail(ResultStatus.Unauthorized);
            var result = await _userManager.SetUserNameAsync(user, request.NewUsername);
            if (!result.Succeeded) result.ToResult();
            await _userManager.UpdateNormalizedUserNameAsync(user);
            return Result<bool>.Success(true);
        }
    }
}