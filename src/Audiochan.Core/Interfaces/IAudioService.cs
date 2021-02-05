using System;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Features.Audios.Models;
using Microsoft.AspNetCore.Http;

namespace Audiochan.Core.Interfaces
{
    public interface IAudioService
    {
        Task<PagedList<AudioViewModel>> GetFeed(string userId, PaginationQuery query, 
            CancellationToken cancellationToken = default);
        Task<PagedList<AudioViewModel>> GetList(GetAudioListQuery query, CancellationToken cancellationToken = default);
        Task<IResult<AudioViewModel>> Get(Guid audioId, CancellationToken cancellationToken = default);
        Task<IResult<AudioViewModel>> GetRandom(CancellationToken cancellationToken = default);
        Task<IResult<AudioViewModel>> Create(UploadAudioRequest request, 
            CancellationToken cancellationToken = default);
        Task<IResult<AudioViewModel>> Update(Guid audioId, UpdateAudioRequest request, 
            CancellationToken cancellationToken = default);
        Task<IResult> Remove(Guid audioId, CancellationToken cancellationToken = default);
        Task<IResult<string>> AddPicture(Guid audioId, IFormFile file,
            CancellationToken cancellationToken = default);
        Task<PagedList<PopularTagViewModel>> GetPopularTags(PaginationQuery paginationQuery,
            CancellationToken cancellationToken = default);
    }
}