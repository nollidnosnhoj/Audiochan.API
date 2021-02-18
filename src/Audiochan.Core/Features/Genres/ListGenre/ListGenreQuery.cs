using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Genres.ListGenre
{
    public enum ListGenresSort
    {
        Alphabetically,
        Popularity
    }
    
    public record ListGenreQuery : IRequest<List<GenreViewModel>>
    {
        public ListGenresSort Sort { get; } = default;
    }

    public class ListGenreQueryHandler : IRequestHandler<ListGenreQuery, List<GenreViewModel>>
    {
        private readonly IApplicationDbContext _dbContext;

        public ListGenreQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<GenreViewModel>> Handle(ListGenreQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Genre> queryable = _dbContext.Genres
                .Include(g => g.Audios);

            switch (request.Sort)
            {
                case ListGenresSort.Popularity:
                    queryable = queryable.OrderByDescending(g => g.Audios.Count);
                    break;
                case ListGenresSort.Alphabetically:
                    queryable = queryable.OrderBy(g => g.Name);
                    break;
            }

            return await queryable.Select(genre => new GenreViewModel
            {
                Id = genre.Id,
                Name = genre.Name,
                Slug = genre.Slug,
                Count = genre.Audios.Count
            }).ToListAsync(cancellationToken);
        }
    }
}