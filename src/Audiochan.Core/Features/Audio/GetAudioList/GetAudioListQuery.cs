using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Mappings;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Features.Audio.GetAudio;
using Audiochan.Core.Interfaces;
using MediatR;

namespace Audiochan.Core.Features.Audio.GetAudioList
{
    public record GetAudioListQuery : PaginationQuery, IRequest<PagedList<AudioViewModel>>
    {
        public string Username { get; init; } = string.Empty;
        public string Tags { get; init; } = string.Empty;
        public string Sort { get; init; } = string.Empty;
        public string Genre { get; init; } = string.Empty;
    }

    public class GetAudioListQueryHandler : IRequestHandler<GetAudioListQuery, PagedList<AudioViewModel>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public GetAudioListQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<PagedList<AudioViewModel>> Handle(GetAudioListQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            var queryable = _dbContext.Audios.DefaultQueryable(currentUserId);

            if (!string.IsNullOrWhiteSpace(request.Username))
                queryable = queryable.Where(a => a.User.UserName == request.Username.Trim().ToLower());

            if (!string.IsNullOrWhiteSpace(request.Tags))
            {
                var parsedTags = request.Tags.Split(',')
                    .Select(t => t.Trim().ToLower())
                    .ToArray();
            
                queryable = queryable.Where(a => a.Tags.Any(t => parsedTags.Contains(t.Id)));
            }

            if (!string.IsNullOrWhiteSpace(request.Genre))
            {
                long genreId = 0;

                if (long.TryParse(request.Genre, out var parsedId))
                    genreId = parsedId;

                queryable = queryable.Where(a => a.GenreId == genreId || a.Genre.Slug == request.Genre.Trim().ToLower());
            }

            queryable = request.Sort switch
            {
                "favorites" => queryable.OrderByDescending(a => a.Favorited.Count),
                _ => queryable.OrderByDescending(a => a.Created)
            };

            return await queryable
                .Select(AudioMappings.Map(currentUserId))
                .Paginate(request, cancellationToken);
        }
    }
}