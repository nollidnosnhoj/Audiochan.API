using System.Linq;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public class FavoriteAudioRepository : BaseRepository<FavoriteAudio>, IFavoriteAudioRepository
    {
        private readonly ICurrentUserService _currentUserService;

        public FavoriteAudioRepository(ApplicationDbContext context, IMapper mapper,
            ICurrentUserService currentUserService) : base(context, mapper)
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
    }
}