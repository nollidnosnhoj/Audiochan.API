using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Users.GetUser
{
    public record GetUserQuery(string Username) : IRequest<IResult<UserViewModel>>
    {
        
    }

    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            string currentUserId = string.Empty;
            CreateMap<User, UserViewModel>()
                .ForMember(dest => dest.AudioCount, opts =>
                    opts.MapFrom(src => src.Audios.Count))
                .ForMember(dest => dest.FollowerCount, opts =>
                    opts.MapFrom(src => src.Followers.Count))
                .ForMember(dest => dest.FollowingCount, opts =>
                    opts.MapFrom(src => src.Followings.Count))
                .ForMember(dest => dest.IsFollowing, opts =>
                    opts.MapFrom(src => 
                        currentUserId != null && currentUserId.Length > 0
                            ? src.Followers.Any(f => f.ObserverId == currentUserId) 
                            : (bool?)null));
        }
    }

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, IResult<UserViewModel>>
    {
        private readonly IUserRepository _userRepository;

        public GetUserQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        public async Task<IResult<UserViewModel>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository
                .GetAsync<UserViewModel>(u => u.UserName == request.Username.Trim().ToLower(), cancellationToken);

            return user == null
                ? Result<UserViewModel>.Fail(ResultError.NotFound)
                : Result<UserViewModel>.Success(user);
        }
    }
}