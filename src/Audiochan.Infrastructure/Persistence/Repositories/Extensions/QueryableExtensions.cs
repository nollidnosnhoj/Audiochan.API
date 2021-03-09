using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<Core.Entities.Audio> DefaultQueryable(this DbSet<Core.Entities.Audio> dbSet, string currentUserId = "")
        {
            return dbSet
                .AsNoTracking()
                .Include(a => a.Tags)
                .Include(a => a.Favorited)
                .Include(a => a.User)
                .Include(a => a.Genre)
                .Where(a => a.UserId == currentUserId || a.IsPublic);
        }

        public static IQueryable<Core.Entities.Audio> FilterBySearchTerm(this IQueryable<Core.Entities.Audio> queryable, string q)
        {
            if (!string.IsNullOrWhiteSpace(q))
            {
                queryable = queryable.Where(a => a.Title.ToLower().Contains(q.ToLower()));
            }

            return queryable;
        }

        public static IQueryable<Core.Entities.Audio> FilterByGenre(this IQueryable<Core.Entities.Audio> queryable, string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                long genreId = 0;

                if (long.TryParse(input, out var parsedId))
                    genreId = parsedId;

                queryable = queryable.Where(a => a.GenreId == genreId || a.Genre.Slug == input.Trim().ToLower());
            }

            return queryable;
        }

        public static IQueryable<Core.Entities.Audio> FilterByTags(this IQueryable<Core.Entities.Audio> queryable, string tags, string delimiter)
        {
            if (!string.IsNullOrWhiteSpace(tags))
            {
                var parsedTags = tags.Split(delimiter)
                    .Select(t => t.Trim().ToLower())
                    .ToArray();
            
                queryable = queryable.Where(a => a.Tags.Any(t => parsedTags.Contains(t.Id)));
            }

            return queryable;
        }

        public static IQueryable<Core.Entities.Audio> Sort(this IQueryable<Core.Entities.Audio> queryable, string sort)
        {
            return sort.ToLower() switch
            {
                "favorites" => queryable.OrderByDescending(a => a.Favorited.Count)
                    .ThenByDescending(a => a.Created),
                "latest" => queryable.OrderByDescending(a => a.Created),
                _ => queryable.OrderByDescending(a => a.Created)
            };
        }
    }
}