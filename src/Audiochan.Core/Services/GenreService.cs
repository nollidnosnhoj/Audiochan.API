using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Services
{
    public class GenreService
    {
        private readonly IApplicationDbContext _dbContext;

        public GenreService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<Genre> GetGenre(string input, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            
            long genreId = 0;
            
            if (long.TryParse(input, out var parseResult))
                genreId = parseResult;

            Expression<Func<Genre, bool>> predicate = genre =>
                genre.Id == genreId || genre.Slug == input.Trim().ToLower();

            return await _dbContext.Genres.Where(predicate).FirstOrDefaultAsync(cancellationToken);
        }
    }
}