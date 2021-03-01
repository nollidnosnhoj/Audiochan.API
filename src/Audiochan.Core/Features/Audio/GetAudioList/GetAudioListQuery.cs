﻿using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Requests;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Features.Audio.GetAudio;
using Audiochan.Core.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

namespace Audiochan.Core.Features.Audio.GetAudioList
{
    public record GetAudioListQuery : AudioListQueryRequest
    {
    }

    public class GetAudioListQueryHandler : IRequestHandler<GetAudioListQuery, PagedList<AudioViewModel>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetAudioListQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService, IMapper mapper)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<PagedList<AudioViewModel>> Handle(GetAudioListQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            return await _dbContext.Audios
                .DefaultQueryable(currentUserId)
                .FilterByGenre(request.Genre)
                .Sort(request.Sort)
                .ProjectTo<AudioViewModel>(_mapper.ConfigurationProvider, new { currentUserId })
                .PaginateAsync(request, cancellationToken);
        }
    }
}