using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Audiochan.Core.Features.Followers.GetFollowers
{
    public record GetFollowersQuery : PaginationQueryRequest<FollowerViewModel>
    {
        public string Username { get; init; }
    }

    public class GetFollowersMappingProfile : Profile
    {
        public GetFollowersMappingProfile()
        {
            CreateMap<FollowedUser, FollowerViewModel>()
                .ForMember(dest => dest.Username, opts =>
                    opts.MapFrom(src => src.Observer.UserName))
                .ForMember(dest => dest.Picture, opts =>
                    opts.MapFrom(src => src.Observer.Picture));
        }
    }

    public class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, PagedList<FollowerViewModel>>
    {
        private readonly IFollowerRepository _followerRepository;

        public GetFollowersQueryHandler(IFollowerRepository followerRepository)
        {
            _followerRepository = followerRepository;
        }

        public async Task<PagedList<FollowerViewModel>> Handle(GetFollowersQuery request,
            CancellationToken cancellationToken)
        {
            return await _followerRepository.ListAsync(
                u => u.Target.UserName == request.Username.Trim().ToLower(), request, cancellationToken);
        }
    }
}