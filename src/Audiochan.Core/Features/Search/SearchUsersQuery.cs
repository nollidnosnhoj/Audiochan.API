using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Users.GetUser;
using Audiochan.Core.Interfaces.Repositories;
using MediatR;

namespace Audiochan.Core.Features.Search
{
    public record SearchUsersQuery : PaginationQueryRequest<UserViewModel>
    {
        public string Q { get; init; }
    }

    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, PagedList<UserViewModel>>
    {
        private readonly IUserRepository _userRepository;

        public SearchUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PagedList<UserViewModel>> Handle(SearchUsersQuery request,
            CancellationToken cancellationToken)
        {
            return await _userRepository.SearchAsync<UserViewModel>(request, cancellationToken);
        }
    }
}