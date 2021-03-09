using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Audiochan.Core.Features.Genres.ListGenre
{
    public record ListGenreQuery : IRequest<List<GenreViewModel>>
    {
        public ListGenresSort Sort { get; } = default;
    }

    public class ListGenreMappingProfile : Profile
    {
        public ListGenreMappingProfile()
        {
            CreateMap<Genre, GenreViewModel>()
                .ForMember(dest => dest.Count, opts =>
                    opts.MapFrom(src => src.Audios.Count));
        }
    }

    public class ListGenreQueryHandler : IRequestHandler<ListGenreQuery, List<GenreViewModel>>
    {
        private readonly IGenreRepository _genreRepository;

        public ListGenreQueryHandler(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public async Task<List<GenreViewModel>> Handle(ListGenreQuery request, CancellationToken cancellationToken)
        {
            return await _genreRepository.ListAsync<GenreViewModel>(request.Sort, cancellationToken);
        }
    }
}