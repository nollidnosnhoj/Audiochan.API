using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Search;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using Audiochan.Infrastructure.Persistence.Repositories.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public class AudioRepository : BaseRepository<Audio>, IAudioRepository
    {
        private readonly ICurrentUserService _currentUserService;

        public AudioRepository(ApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService) : base(context, mapper)
        {
            _currentUserService = currentUserService;
        }

        protected override IQueryable<Audio> BaseQueryable => Context.Set<Audio>()
            .Include(a => a.Tags)
            .Include(a => a.Favorited)
            .Include(a => a.User)
            .Include(a => a.Genre);

        public async Task<TDto> GetAsync<TDto>(long id, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();

            return await BaseQueryable
                .AsNoTracking()
                .Where(a => a.UserId == currentUserId || a.IsPublic)
                .Where(x => x.Id == id)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, new {currentUserId})
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<TDto> RandomAsync<TDto>(CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            return await BaseQueryable
                .AsNoTracking()
                .Where(a => a.UserId == currentUserId || a.IsPublic)
                .OrderBy(a => Guid.NewGuid())
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, new {currentUserId})
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<PagedList<TDto>> ListAsync<TDto>(AudioListQueryRequest query,
            Expression<Func<Audio, bool>> whereExpression = null,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();

            var queryable = BaseQueryable
                .AsNoTracking()
                .Where(a => a.UserId == currentUserId || a.IsPublic);

            if (whereExpression is not null)
            {
                queryable = queryable.Where(whereExpression);
            }

            return await queryable.FilterByGenre(query.Genre)
                .Sort(query.Sort)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, new {currentUserId})
                .PaginateAsync(query.Page, query.Size, cancellationToken);
        }

        public async Task<PagedList<TDto>> SearchAsync<TDto>(SearchAudiosQuery query,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();

            return await BaseQueryable
                .AsNoTracking()
                .Where(a => a.UserId == currentUserId || a.IsPublic)
                .FilterBySearchTerm(query.Q)
                .FilterByGenre(query.Genre)
                .FilterByTags(query.Tags, ",")
                .Sort(query.Sort)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, new {currentUserId})
                .PaginateAsync(query.Page, query.Size, cancellationToken);
        }

        public async Task<PagedList<TDto>> FeedAsync<TDto>(string userId, int page, int size,
            CancellationToken cancellationToken = default)
        {
            var followedIds = await Context.FollowedUsers
                .AsNoTracking()
                .Where(user => user.ObserverId == userId)
                .Select(user => user.TargetId)
                .ToListAsync(cancellationToken);

            return await BaseQueryable
                .AsNoTracking()
                .Where(a => a.UserId == userId || a.IsPublic)
                .Where(a => followedIds.Contains(a.UserId))
                .OrderByDescending(a => a.Created)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, new {currentUserId = userId})
                .PaginateAsync(page, size, cancellationToken);
        }
    }
}