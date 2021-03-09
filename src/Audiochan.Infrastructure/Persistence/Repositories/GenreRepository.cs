using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public class GenreRepository : BaseRepository<Genre>, IGenreRepository
    {
        public GenreRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        protected override IQueryable<Genre> BaseQueryable => Context.Set<Genre>();


        public async Task<Genre> GetAsync(string genre, CancellationToken cancellationToken = default)
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

        public async Task<List<TDto>> ListAsync<TDto>(ListGenresSort sort, CancellationToken cancellationToken = default)
        {
            IQueryable<Genre> queryable = BaseQueryable
                .Include(g => g.Audios);

            switch (sort)
            {
                case ListGenresSort.Popularity:
                    queryable = queryable.OrderByDescending(g => g.Audios.Count);
                    break;
                case ListGenresSort.Alphabetically:
                    queryable = queryable.OrderBy(g => g.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }

            return await queryable
                .ProjectTo<TDto>(Mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}