using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Users.GetUser;
using Audiochan.Core.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

namespace Audiochan.Core.Features.Search
{
    public record SearchUsersQuery : PaginationQueryRequest<UserViewModel>
    {
        public string Q { get; init; }
    }
    
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, PagedList<UserViewModel>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public SearchUsersQueryHandler(IApplicationDbContext dbContext, IMapper mapper, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PagedList<UserViewModel>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            return await _dbContext.Users
                .Where(u => u.UserName.Contains(request.Q.ToLower()))
                .ProjectTo<UserViewModel>(_mapper.ConfigurationProvider, new {currentUserId})
                .PaginateAsync(request, cancellationToken);
        }
    }
}