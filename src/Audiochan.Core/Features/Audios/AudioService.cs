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
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Audios.Builders;
using Audiochan.Core.Features.Audios.Models;
using Audiochan.Core.Features.Audios.Extensions;
using Audiochan.Core.Features.Audios.Mappings;
using Audiochan.Core.Interfaces;
using Microsoft.AspNetCore.Http;
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

        public AudioService(IDatabaseContext dbContext, 
            ICurrentUserService currentUserService,
            IGenreService genreService, 
            IStorageService storageService, 
            IAudioMetadataService audioMetadataService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _genreService = genreService;
            _storageService = storageService;
            _audioMetadataService = audioMetadataService;
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
                .AsNoTracking()
                .Include(a => a.Favorited)
                .Include(a => a.User)
                .FilterVisibility(userId)
                .Where(a => followedIds.Contains(a.UserId))
                .Distinct()
                .Select(AudioDetailMapping.Map(userId))
                .OrderByDescending(a => a.Created)
                .Paginate(query, cancellationToken);
        }

        public async Task<PagedList<AudioViewModel>> GetList(GetAudioListQuery query, 
            CancellationToken cancellationToken = default)
        {
            // Get userId of the current user
            var currentUserId = _currentUserService.GetUserId();

            // Build query
            var queryable = _dbContext.Audios
                .AsNoTracking()
                .Include(a => a.Favorited)
                .Include(a => a.User)
                .Include(a => a.Genre)
                .FilterVisibility(currentUserId);

            if (!string.IsNullOrWhiteSpace(query.Username))
                queryable = queryable.Where(a => a.User.UserName == query.Username);
            
            if (!string.IsNullOrWhiteSpace(query.Tags))
                queryable = queryable.FilterByTags(query.Tags);

            if (!string.IsNullOrWhiteSpace(query.Genre))
                queryable = queryable.FilterByGenre(query.Genre);
            
            queryable = queryable.Sort(query.Sort.ToLower());

            return await queryable
                .Select(AudioDetailMapping.Map(currentUserId))
                .Paginate(query, cancellationToken);
        }

        public async Task<IResult<AudioViewModel>> Get(Guid audioId, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            
            var audio = await _dbContext.Audios
                .AsNoTracking()
                .Include(a => a.Favorited)
                .Include(a => a.Tags)
                .Include(a => a.User)
                .Include(a => a.Genre)
                .FilterVisibility(currentUserId)
                .Where(x => x.Id == audioId)
                .Select(AudioDetailMapping.Map(currentUserId))
                .SingleOrDefaultAsync(cancellationToken);

            return audio == null 
                ? Result<AudioViewModel>.Fail(ResultStatus.NotFound) 
                : Result<AudioViewModel>.Success(audio);
        }

        public async Task<IResult<AudioViewModel>> GetRandom(CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            var audio = await _dbContext.Audios
                .AsNoTracking()
                .Include(a => a.Favorited)
                .Include(a => a.Tags)
                .Include(a => a.User)
                .Include(a => a.Genre)
                .FilterVisibility(currentUserId)
                .OrderBy(a => Guid.NewGuid())
                .Select(AudioDetailMapping.Map(currentUserId))
                .SingleOrDefaultAsync(cancellationToken);

            return audio == null 
                ? Result<AudioViewModel>.Fail(ResultStatus.NotFound) 
                : Result<AudioViewModel>.Success(audio);
        }

        public async Task<IResult<AudioViewModel>> Create(UploadAudioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var id = request.Id;
            var title = request.Title;
            var extension = Path.GetExtension(request.FileName);
            var duration = request.Duration;

            if (request.File is not null)
            {
                extension = Path.GetExtension(request.File.FileName);
                title = string.IsNullOrWhiteSpace(title)
                    ? Path.GetFileNameWithoutExtension(request.File.FileName)
                    : request.Title;
                duration = _audioMetadataService.GetDuration(request.File);
            }
            
            var audioBuilder = new AudioBuilder()
                .AddId(id)
                .AddTitle(title)
                .AddDescription(request.Description)
                .AddDuration(duration)
                .AddFileExtension(extension)
                .SetToPublic(request.IsPublic)
                .SetToLoop(request.IsLoop);
            
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var currentUser = await _dbContext.Users
                    .SingleOrDefaultAsync(u => u.Id == _currentUserService.GetUserId(), cancellationToken);
                
                var genre = await _genreService.GetGenre(request.Genre ?? "misc", cancellationToken);
                
                if (genre == null)
                    return Result<AudioViewModel>
                        .Fail(ResultStatus.BadRequest, "Genre does not exist.");
                
                var tags = request.Tags.Count > 0
                    ? await CreateNewTags(request.Tags, cancellationToken)
                    : new List<Tag>();

                var audio = audioBuilder
                    .AddGenre(genre)
                    .AddTags(tags)
                    .AddUser(currentUser)
                    .Build();
                
                id = audio.Id;
                await _dbContext.Audios.AddAsync(audio, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (request.File is not null)
                {
                    var memoryStream = new MemoryStream();
                    await request.File.CopyToAsync(memoryStream, cancellationToken);
                    await _storageService.SaveAsync(
                        container: ContainerConstants.Audios,
                        blobName: Path.Combine(id.ToString(), $"source{extension}"), 
                        stream: memoryStream,
                        overwrite: false,
                        cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result<AudioViewModel>.Success(audio.MapToDetail(currentUser.Id));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                var path = Path.Combine(ContainerConstants.Audios, id.ToString(), $"source{extension}");
                await _storageService.RemoveAsync(path, cancellationToken);
                throw; 
            }
        }

        public async Task<IResult<AudioViewModel>> Update(Guid audioId, UpdateAudioRequest request, 
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

                if (!string.IsNullOrWhiteSpace(request.Genre) && audio.Genre.Name != request.Genre)
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

        public async Task<IResult> Remove(Guid audioId, CancellationToken cancellationToken = default)
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
            var audioPath = Path.Combine(ContainerConstants.Audios, audio.Id.ToString(), $"source{audio.FileExt}");
            var removeAudioBlobTask = _storageService.RemoveAsync(audioPath, cancellationToken);
            // TODO: Work on removing audio picture
            // var removeImageBlobTask = _storageService.RemoveAsync(audio.PictureUrl, cancellationToken);
            await Task.WhenAll(removeEntityFromDatabaseTask, removeAudioBlobTask);
            return Result.Success();
        }

        public Task<IResult<string>> AddPicture(Guid audioId, IFormFile file,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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