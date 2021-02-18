using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audio
{
    public static class QueryableExtensions
    {
        public static IQueryable<Entities.Audio> DefaultQueryable(this DbSet<Entities.Audio> dbSet, string currentUserId = "")
        {
            return dbSet
                .AsNoTracking()
                .Include(a => a.Tags)
                .Include(a => a.Favorited)
                .Include(a => a.User)
                .Include(a => a.Genre)
                .Where(a => a.UserId == currentUserId || a.IsPublic);
        }
    }
}