using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public class GenreRepository : BaseRepository<Genre>, IGenreRepository
    {
        public GenreRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        protected override IQueryable<Genre> BaseQueryable => Context.Set<Genre>();


        public async Task<Genre> GetByInputAsync(string genre, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(genre))
                return null;

            long genreId = 0;

            if (long.TryParse(genre, out var parseResult))
                genreId = parseResult;

            Expression<Func<Genre, bool>> predicate = g =>
                g.Id == genreId || g.Slug == genre.Trim().ToLower();

            return await BaseQueryable.Where(predicate).FirstOrDefaultAsync(cancellationToken);
        }
    }
}