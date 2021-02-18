using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Favorites.Audios.SetFavorite
{
    public record SetFavoriteCommand(string UserId, long AudioId, bool IsFavoriting) : IRequest<IResult<bool>>
    {
        
    }

    public class SetFavoriteCommandHandler : IRequestHandler<SetFavoriteCommand, IResult<bool>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public SetFavoriteCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }


        public async Task<IResult<bool>> Handle(SetFavoriteCommand request, CancellationToken cancellationToken)
        {
            if (!await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == request.UserId, cancellationToken))
                return Result<bool>.Fail(ResultStatus.Unauthorized);

            var audio = await _dbContext.Audios
                .AsNoTracking()
                .Select(a => new {a.Id, a.UserId})
                .SingleOrDefaultAsync(a => a.Id == request.AudioId, cancellationToken);
            
            if (audio == null) 
                return Result<bool>.Fail(ResultStatus.NotFound);
            
            if (audio.UserId == request.UserId)
                return Result<bool>.Fail(ResultStatus.Forbidden);
            
            var favorite =
                await _dbContext.FavoriteAudios
                    .SingleOrDefaultAsync(fa => fa.AudioId == request.AudioId && fa.UserId == request.UserId,
                        cancellationToken);

            if (favorite != null && !request.IsFavoriting)
            {
                _dbContext.FavoriteAudios.Remove(favorite);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(false);
            }

            if (favorite == null && request.IsFavoriting)
            {
                favorite = new FavoriteAudio {UserId = request.UserId, AudioId = request.AudioId};
                await _dbContext.FavoriteAudios.AddAsync(favorite, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }

            return Result<bool>.Success(favorite != null);
        }
    }
}