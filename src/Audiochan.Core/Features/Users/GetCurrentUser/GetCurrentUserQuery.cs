using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Users.GetCurrentUser
{
    public record GetCurrentUserQuery : IRequest<IResult<CurrentUserViewModel>>
    {
        
    }

    public class CurrentUserMappingProfile : Profile
    {
        public CurrentUserMappingProfile()
        {
            CreateMap<User, CurrentUserViewModel>();
        }
    }

    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, IResult<CurrentUserViewModel>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;

        public GetCurrentUserQueryHandler(ICurrentUserService currentUserService, IUserRepository userRepository)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        public async Task<IResult<CurrentUserViewModel>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            var user = await _userRepository
                .GetAsync<CurrentUserViewModel>(u => u.Id == currentUserId, cancellationToken);

            return user == null
                ? Result<CurrentUserViewModel>.Fail(ResultError.Unauthorized)
                : Result<CurrentUserViewModel>.Success(user);
        }
    }
}