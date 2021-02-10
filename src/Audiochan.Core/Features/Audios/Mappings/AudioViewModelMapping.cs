using System;
using System.Linq;
using System.Linq.Expressions;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Audios.Models;
using Audiochan.Core.Features.Genres.Models;
using Audiochan.Core.Features.Users.Models;

namespace Audiochan.Core.Features.Audios.Mappings
{
    public static class AudioViewModelMapping
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
                Tags = string.Join(' ', audio.Tags.Select(tag => tag.Id)),
                FavoriteCount = audio.Favorited.Count,
                IsFavorited = currentUserId != null 
                              && currentUserId.Length > 0
                              && audio.Favorited.Any(f => f.UserId == currentUserId),
                Created = audio.Created,
                Updated = audio.LastModified,
                Genre = audio.Genre != null 
                    ? new GenreDto(audio.Genre.Id, audio.Genre.Name, audio.Genre.Slug) 
                    : null,
                User = new UserViewModel
                {
                    Id = audio.User.Id,
                    Username = audio.User.UserName,
                },
                UploadId = audio.UploadId
            };
        }

        public static AudioViewModel MapToDetail(this Audio audio, string currentUserId) => 
            Map(currentUserId).Compile().Invoke(audio);
    }
}