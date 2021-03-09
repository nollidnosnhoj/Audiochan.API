using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Search;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ICurrentUserService _currentUserService;

        public UserRepository(ApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService) :
            base(context, mapper)
        {
            _currentUserService = currentUserService;
        }

        protected override IQueryable<User> BaseQueryable => Context.Set<User>()
            .Include(u => u.Followers)
            .Include(u => u.Followings)
            .Include(u => u.Audios);

        public async Task<TDto> GetAsync<TDto>(Expression<Func<User, bool>> expression,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();

            return await BaseQueryable
                .AsNoTracking()
                .Where(expression)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, new {currentUserId})
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<PagedList<TDto>> SearchAsync<TDto>(SearchUsersQuery query,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            return await BaseQueryable
                .Where(u => u.UserName.Contains(query.Q.ToLower()))
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, new {currentUserId})
                .PaginateAsync(query.Page, query.Size, cancellationToken);
        }
    }
}