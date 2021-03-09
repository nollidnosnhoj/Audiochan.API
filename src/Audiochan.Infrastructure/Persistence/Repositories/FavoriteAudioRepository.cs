using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public class FavoriteAudioRepository : BaseRepository<FavoriteAudio>, IFavoriteAudioRepository
    {
        private readonly ICurrentUserService _currentUserService;
        
        public FavoriteAudioRepository(ApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService) : base(context, mapper)
        {
            _currentUserService = currentUserService;
        }

        protected override IQueryable<FavoriteAudio> BaseQueryable => Context.Set<FavoriteAudio>()
            .Include(fa => fa.User)
            .Include(fa => fa.Audio)
            .ThenInclude(a => a.User)
            .Include(fa => fa.Audio)
            .ThenInclude(a => a.Tags)
            .Include(fa => fa.Audio)
            .ThenInclude(a => a.Genre)
            .Include(fa => fa.Audio)
            .ThenInclude(a => a.Favorited);


        public async Task<PagedList<TDto>> ListAsync<TDto>(string username, PaginationQueryRequest<TDto> paginationQuery,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            return await BaseQueryable.AsNoTracking()
                .Where(fa => fa.User.UserName == username.Trim().ToLower())
                .OrderByDescending(fa => fa.Created)
                .Select(fa => fa.Audio)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, new { currentUserId })
                .PaginateAsync(paginationQuery, cancellationToken);
        }
    }
}