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

        public static IQueryable<Entities.Audio> FilterBySearchTerm(this IQueryable<Entities.Audio> queryable, string q)
        {
            if (!string.IsNullOrWhiteSpace(q))
            {
                queryable = queryable.Where(a => a.Title.ToLower().Contains(q.ToLower()));
            }

            return queryable;
        }

        public static IQueryable<Entities.Audio> FilterByGenre(this IQueryable<Entities.Audio> queryable, string input)
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

        public static IQueryable<Entities.Audio> FilterByTags(this IQueryable<Entities.Audio> queryable, string tags)
        {
            if (!string.IsNullOrWhiteSpace(tags))
            {
                var parsedTags = tags.Split(',')
                    .Select(t => t.Trim().ToLower())
                    .ToArray();
            
                queryable = queryable.Where(a => a.Tags.Any(t => parsedTags.Contains(t.Id)));
            }

            return queryable;
        }

        public static IQueryable<Entities.Audio> Sort(this IQueryable<Entities.Audio> queryable, string sort)
        {
            return sort.ToLower() switch
            {
                "favorites" => queryable.OrderByDescending(a => a.Favorited.Count),
                _ => queryable.OrderByDescending(a => a.Created)
            };
        }
    }
}