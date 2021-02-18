using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Genres.GetGenre
{
    public record GetGenreQuery(string Input) : IRequest<Genre>
    {

    }

    public class GetGenreQueryHandler : IRequestHandler<GetGenreQuery, Genre>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetGenreQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Genre> Handle(GetGenreQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
                return null;
            
            long genreId = 0;
            
            if (long.TryParse(request.Input, out var parseResult))
                genreId = parseResult;

            Expression<Func<Genre, bool>> predicate = genre =>
                genre.Id == genreId || genre.Slug == request.Input.Trim().ToLower();

            return await _dbContext.Genres.Where(predicate).FirstOrDefaultAsync(cancellationToken);
        }
    }
}