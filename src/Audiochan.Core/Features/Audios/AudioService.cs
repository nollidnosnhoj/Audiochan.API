using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Result;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Audios.Models;
using Audiochan.Core.Features.Audios.Extensions;
using Audiochan.Core.Features.Audios.Mappings;
using Audiochan.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audios
{
    public class AudioService : IAudioService
    {
        private readonly IDatabaseContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGenreService _genreService;
        private readonly IStorageService _storageService;
        private readonly IAudioMetadataService _audioMetadataService;
        private readonly IImageService _imageService;

        public AudioService(IDatabaseContext dbContext, 
            ICurrentUserService currentUserService,
            IGenreService genreService, 
            IStorageService storageService, 
            IAudioMetadataService audioMetadataService, 
            IImageService imageService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _genreService = genreService;
            _storageService = storageService;
            _audioMetadataService = audioMetadataService;
            _imageService = imageService;
        }

        public async Task<PagedList<AudioViewModel>> GetFeed(string userId, PaginationQuery query,
            CancellationToken cancellationToken = default)
        {
            // Get the user Ids of the followed users
            var followedIds = await _dbContext.FollowedUsers
                .AsNoTracking()
                .Where(user => user.ObserverId == userId)
                .Select(user => user.TargetId)
                .ToListAsync(cancellationToken);

            return await _dbContext.Audios
                .DefaultQueryable(userId)
                .Where(a => followedIds.Contains(a.UserId))
                .Distinct()
                .Select(AudioViewModelMapping.Map(userId))
                .OrderByDescending(a => a.Created)
                .Paginate(query, cancellationToken);
        }

        public async Task<PagedList<AudioViewModel>> GetList(GetAudioListQuery query, 
            CancellationToken cancellationToken = default)
        {
            // Get userId of the current user
            var currentUserId = _currentUserService.GetUserId();
            
            return await _dbContext.Audios
                .DefaultQueryable(currentUserId)
                .FilterByUsername(query.Username)
                .FilterByTags(query.Tags)
                .FilterByGenre(query.Genre)
                .Sort(query.Sort.ToLower())
                .Select(AudioViewModelMapping.Map(currentUserId))
                .Paginate(query, cancellationToken);
        }

        public async Task<IResult<AudioViewModel>> Get(long audioId, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            
            var audio = await _dbContext.Audios
                .DefaultQueryable(currentUserId)
                .Where(x => x.Id == audioId)
                .Select(AudioViewModelMapping.Map(currentUserId))
                .SingleOrDefaultAsync(cancellationToken);

            return audio == null 
                ? Result<AudioViewModel>.Fail(ResultStatus.NotFound) 
                : Result<AudioViewModel>.Success(audio);
        }

        public async Task<IResult<AudioViewModel>> GetRandom(CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            var audio = await _dbContext.Audios
                .DefaultQueryable(currentUserId)
                .OrderBy(a => Guid.NewGuid())
                .Select(AudioViewModelMapping.Map(currentUserId))
                .SingleOrDefaultAsync(cancellationToken);

            return audio == null 
                ? Result<AudioViewModel>.Fail(ResultStatus.NotFound) 
                : Result<AudioViewModel>.Success(audio);
        }

        public async Task<IResult<AudioViewModel>> Create(UploadAudioRequest request, 
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var audio = new Audio
            {
                UploadId = request.UploadId,
                Title = !string.IsNullOrWhiteSpace(request.Title)
                    ? request.Title
                    : Path.GetFileNameWithoutExtension(request.FileName),
                Description = request.Description,
                Duration = request.Duration,
                FileExt = Path.GetExtension(request.FileName),
                IsPublic = request.IsPublic ?? true,
                IsLoop = request.IsLoop ?? false,
                FileSize = request.FileSize
            };
                
            if (request.File is not null)
            {
                audio.FileSize = request.File.Length;
                audio.FileExt = Path.GetExtension(request.File.FileName);
                audio.Title = string.IsNullOrWhiteSpace(audio.Title)
                    ? Path.GetFileNameWithoutExtension(request.File.FileName)
                    : audio.Title;
                audio.Duration = _audioMetadataService.GetDuration(request.File);
            }
                
            try
            {
                audio.User = await _dbContext.Users
                    .SingleOrDefaultAsync(u => u.Id == _currentUserService.GetUserId(), cancellationToken);
                audio.Genre = await _genreService.GetGenre(request.Genre, cancellationToken);
                audio.Tags = request.Tags.Count > 0
                    ? await CreateNewTags(request.Tags, cancellationToken)
                    : new List<Tag>();

                await _dbContext.Audios.AddAsync(audio, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (request.File is not null)
                {
                    audio.UploadId = Guid.NewGuid();
                    var memoryStream = new MemoryStream();
                    await request.File.CopyToAsync(memoryStream, cancellationToken);
                    var blobRequest = new SaveBlobRequest
                    {
                        Container = ContainerConstants.Audios,
                        BlobName = audio.UploadId + audio.FileExt,
                        OriginalFileName = request.File.FileName
                    };
                    blobRequest.Metadata.Add("UserId", audio.User.Id);
                    await _storageService.SaveAsync(memoryStream, blobRequest, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return Result<AudioViewModel>.Success(audio.MapToDetail(audio.User.Id));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                var blobName = Path.Combine(audio.UploadId.ToString(), "source" + audio.FileExt);
                await _storageService.RemoveAsync(ContainerConstants.Audios, blobName, cancellationToken);
                throw; 
            }
        }

        public async Task<IResult<AudioViewModel>> Update(long audioId, UpdateAudioRequest request, 
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var currentUserId = _currentUserService.GetUserId();

                var audio = await _dbContext.Audios
                    .Include(a => a.Favorited)
                    .Include(a => a.User)
                    .Include(a => a.Tags)
                    .Include(a => a.Genre)
                    .SingleOrDefaultAsync(a => a.Id == audioId, cancellationToken);

                if (audio == null) return Result<AudioViewModel>.Fail(ResultStatus.NotFound);

                if (audio.UserId != currentUserId) return Result<AudioViewModel>.Fail(ResultStatus.Forbidden);

                var newTags = await CreateNewTags(request.Tags, cancellationToken);

                audio.Title = request.Title ?? audio.Title;
                audio.Description = request.Description ?? audio.Description;
                audio.IsPublic = request.IsPublic ?? audio.IsPublic;
                audio.IsLoop = request.IsLoop ?? audio.IsLoop;

                if (!string.IsNullOrWhiteSpace(request.Genre) && (audio.Genre?.Slug ?? "") != request.Genre)
                {
                    var genre = await _genreService.GetGenre(request.Genre, cancellationToken);

                    if (genre == null)
                        return Result<AudioViewModel>
                            .Fail(ResultStatus.BadRequest, "Genre does not exist.");

                    audio.Genre = genre!;
                }

                foreach (var audioTag in audio.Tags)
                {
                    if (newTags.All(t => t.Id != audioTag.Id))
                        audio.Tags.Remove(audioTag);
                }

                foreach (var newTag in newTags)
                {
                    if (audio.Tags.All(t => t.Id != newTag.Id))
                        audio.Tags.Add(newTag);
                }

                _dbContext.Audios.Update(audio);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result<AudioViewModel>.Success(audio.MapToDetail(currentUserId));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<IResult> Remove(long audioId, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();

            var audio = await _dbContext.Audios
                .SingleOrDefaultAsync(a => a.Id == audioId, cancellationToken);

            if (audio == null)
                return Result.Fail(ResultStatus.NotFound);

            if (audio.UserId != currentUserId)
                return Result.Fail(ResultStatus.Forbidden);

            _dbContext.Audios.Remove(audio);
            var removeEntityFromDatabaseTask = _dbContext.SaveChangesAsync(cancellationToken);
            var audioBlobName = audio.UploadId + audio.FileExt;
            var removeAudioBlobTask = _storageService.RemoveAsync(ContainerConstants.Audios, audioBlobName, cancellationToken);
            var removeImageBlobTask = _storageService.RemoveAsync(audio.Picture, cancellationToken);
            await Task.WhenAll(removeEntityFromDatabaseTask, removeAudioBlobTask, removeImageBlobTask);
            return Result.Success();
        }

        public async Task<IResult<string>> AddPicture(long audioId, string data, CancellationToken cancellationToken = default)
        {
            var blobName = audioId + "_" + Guid.NewGuid().ToString("N") + ".jpg";
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var currentUserId = _currentUserService.GetUserId();

                var audio = await _dbContext.Audios
                    .Include(a => a.Favorited)
                    .Include(a => a.User)
                    .Include(a => a.Tags)
                    .Include(a => a.Genre)
                    .SingleOrDefaultAsync(a => a.Id == audioId, cancellationToken);

                if (audio == null) return Result<string>.Fail(ResultStatus.NotFound);

                if (audio.UserId != currentUserId) return Result<string>.Fail(ResultStatus.Forbidden);
                var response = await _imageService.UploadImage(data, PictureType.Audio, blobName, cancellationToken);
                audio.Picture = response.Path;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result<string>.Success(response.Url);
            }
            catch (Exception)
            {
                var task1 = transaction.RollbackAsync(cancellationToken);
                var task2 = _imageService.RemoveImage(PictureType.Audio, blobName, cancellationToken);
                await Task.WhenAll(task1, task2);
                throw;
            }
        }
        
        public async Task<PagedList<PopularTagViewModel>>  GetPopularTags(
            PaginationQuery paginationQuery
            , CancellationToken cancellationToken = default)
        {
            return await _dbContext.Tags
                .AsNoTracking()
                .Include(t => t.Audios)
                .Select(t => new PopularTagViewModel{ Tag = t.Id, Count = t.Audios.Count })
                .OrderByDescending(dto => dto.Count)
                .Paginate(paginationQuery, cancellationToken);
        }
        
        private async Task<List<Tag>> CreateNewTags(IEnumerable<string> requestedTags, 
            CancellationToken cancellationToken = default)
        {
            var taggifyTags = requestedTags.FormatTags();

            var tags = await _dbContext.Tags
                .Where(tag => taggifyTags.Contains(tag.Id))
                .ToListAsync(cancellationToken);
            
            foreach (var tag in taggifyTags.Where(tag => tags.All(t => t.Id != tag)))
            {
                tags.Add(new Tag{Id = tag});
            }

            return tags;
        }
    }
}