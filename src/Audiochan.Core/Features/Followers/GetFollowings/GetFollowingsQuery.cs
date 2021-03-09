using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Audiochan.Core.Features.Followers.GetFollowings
{
    public record GetFollowingsQuery : PaginationQueryRequest<FollowingViewModel>
    {
        public string Username { get; init; }
    }

    public class GetFollowingsMappingProfile : Profile
    {
        public GetFollowingsMappingProfile()
        {
            CreateMap<FollowedUser, FollowingViewModel>()
                .ForMember(dest => dest.Username, opts =>
                    opts.MapFrom(src => src.Target.UserName))
                .ForMember(dest => dest.Picture, opts =>
                    opts.MapFrom(src => src.Target.Picture));
        }
    }

    public class GetFollowingsQueryHandler : IRequestHandler<GetFollowingsQuery, PagedList<FollowingViewModel>>
    {
        private readonly IFollowerRepository _followerRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetFollowingsQueryHandler(IFollowerRepository followerRepository, ICurrentUserService currentUserService)
        {
            _followerRepository = followerRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedList<FollowingViewModel>> Handle(GetFollowingsQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            return await _followerRepository.PagedListAsync<FollowingViewModel>(
                request.Page,
                request.Size,
                u => u.Observer.UserName == request.Username.Trim().ToLower(),
                u => u.Created,
                true,
                false,
                new {currentUserId},
                cancellationToken);
        }
    }
}