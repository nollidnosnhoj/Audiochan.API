using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public class FollowerRepository : BaseRepository<FollowedUser>, IFollowerRepository
    {
        public FollowerRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        protected override IQueryable<FollowedUser> BaseQueryable => Context.Set<FollowedUser>()
            .Include(u => u.Target)
            .Include(u => u.Observer);


        public async Task<PagedList<TDto>> ListAsync<TDto>(Expression<Func<FollowedUser, bool>> expression, 
            PaginationQueryRequest<TDto> paginationQuery, CancellationToken cancellationToken = default)
        {
            return await BaseQueryable
                .AsNoTracking()
                .Where(expression)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider)
                .PaginateAsync(paginationQuery, cancellationToken);
        }
    }
}