using System;
using System.Linq;
using System.Linq.Expressions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Audio.GetAudio;

namespace Audiochan.Core.Common.Mappings
{
    public static class AudioMappings
    {
        public static Expression<Func<Audio, AudioViewModel>> Map(string currentUserId)
        {
            return audio => new AudioViewModel
            {
                Id = audio.Id,
                Title = audio.Title,
                Description = audio.Description,
                IsPublic = audio.IsPublic,
                IsLoop = audio.IsLoop,
                Duration = audio.Duration,
                FileSize = audio.FileSize,
                FileExt = audio.FileExt,
                Picture = audio.Picture,
                Tags = audio.Tags.Select(tag => tag.Id).ToArray(),
                FavoriteCount = audio.Favorited.Count,
                IsFavorited = currentUserId != null 
                              && currentUserId.Length > 0
                              && audio.Favorited.Any(f => f.UserId == currentUserId),
                Created = audio.Created,
                Updated = audio.LastModified,
                Genre = audio.Genre != null 
                    ? new AudioViewModel.GenreDto(audio.Genre.Id, audio.Genre.Name, audio.Genre.Slug) 
                    : null,
                User = new UserDto(audio.User.Id, audio.User.UserName, audio.User.Picture, audio.User.Followers.Any(x => x.ObserverId == currentUserId)),
                UploadId = audio.UploadId
            };
        }
        
        public static AudioViewModel MapToDetail(this Audio audio, string currentUserId) => 
            Map(currentUserId).Compile().Invoke(audio);
    }
}