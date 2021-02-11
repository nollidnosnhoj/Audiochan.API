using System.Linq;
using System.Text;
using Audiochan.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audios.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<Audio> DefaultQueryable(this DbSet<Audio> dbSet, string currentUserId = "")
        {
            return dbSet
                .AsNoTracking()
                .Include(a => a.Tags)
                .Include(a => a.Favorited)
                .Include(a => a.User)
                .Include(a => a.Genre)
                .Where(a => a.UserId == currentUserId || a.IsPublic);
        }

        public static IQueryable<Audio> FilterByUsername(this IQueryable<Audio> queryable, string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return queryable;
            
            return queryable.Where(a => a.User.UserName == username.ToLower());
        }

        public static IQueryable<Audio> FilterByGenre(this IQueryable<Audio> queryable, string genreInput)
        {
            if (string.IsNullOrWhiteSpace(genreInput)) return queryable;

            long genreId = 0;

            if (long.TryParse(genreInput, out var parsedId))
                genreId = parsedId;

            return queryable.Where(a => a.GenreId == genreId || a.Genre.Slug == genreInput.Trim().ToLower());
        }
        
        public static IQueryable<Audio> FilterByTags(this IQueryable<Audio> queryable, string tags)
        {
            if (string.IsNullOrWhiteSpace(tags)) return queryable;
            
            var parsedTags = tags.Split(',')
                .Select(t => t.Trim().ToLower())
                .ToArray();
            
            return queryable.Where(a => a.Tags.Any(t => parsedTags.Contains(t.Id)));
        }
        
        public static IQueryable<Audio> Sort(this IQueryable<Audio> queryable, string orderBy)
        {
            return orderBy switch
            {
                "favorites" => 
                    queryable.OrderByDescending(a => a.Favorited.Count),
                _ => 
                    queryable.OrderByDescending(a => a.Created)
            };
        }
    }
}