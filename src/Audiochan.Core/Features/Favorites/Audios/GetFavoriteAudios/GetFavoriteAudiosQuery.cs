using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Mappings;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Features.Audio.GetAudio;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Favorites.Audios.GetFavoriteAudios
{
    public record GetFavoriteAudiosQuery : PaginationQuery<AudioViewModel>
    {
        public string Username { get; init; }
    }

    public class GetFavoriteAudiosQueryHandler : IRequestHandler<GetFavoriteAudiosQuery, PagedList<AudioViewModel>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public GetFavoriteAudiosQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }
        
        public async Task<PagedList<AudioViewModel>> Handle(GetFavoriteAudiosQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            return await _dbContext.FavoriteAudios
                .AsNoTracking()
                .Include(fa => fa.User)
                .Include(fa => fa.Audio)
                .ThenInclude(a => a.User)
                .Include(fa => fa.Audio)
                .ThenInclude(a => a.Favorited)
                .Where(fa => fa.User.UserName == request.Username.Trim().ToLower())
                .OrderByDescending(fa => fa.Created)
                .Select(fa => fa.Audio)
                .Select(AudioMappings.Map(currentUserId))
                .Paginate(request, cancellationToken);
        }
    }
}