using System.Linq;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
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
    }
}