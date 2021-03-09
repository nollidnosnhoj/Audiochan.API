using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        public GetFollowingsQueryHandler(IFollowerRepository followerRepository)
        {
            _followerRepository = followerRepository;
        }

        public async Task<PagedList<FollowingViewModel>> Handle(GetFollowingsQuery request, CancellationToken cancellationToken)
        {
            return await _followerRepository.ListAsync(
                u => u.Observer.UserName == request.Username.Trim().ToLower(), request, cancellationToken);
        }
    }
}