﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Audiochan.Core.Entities.Base;

namespace Audiochan.Core.Entities
{
    public class Audio : BaseEntity
    {
        public Audio()
        {
            this.Tags = new HashSet<Tag>();
            this.Favorited = new HashSet<FavoriteAudio>();
        }
        
        public Audio(Guid uploadId, string fileName, int fileSize, int duration, User user) : this()
        {
            if (uploadId == Guid.Empty)
                throw new ArgumentException("UploadId cannot be empty.", nameof(uploadId));
            
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var fileExtension = Path.GetExtension(fileName);
            
            if (string.IsNullOrEmpty(fileExtension))
                throw new ArgumentException("File name does not have file extension", nameof(fileName));
            
            this.UploadId = uploadId;
            this.FileExt = fileExtension;
            this.FileSize = fileSize;
            this.Duration = duration;
            this.Title = Path.GetFileNameWithoutExtension(fileName);
            this.User = user ?? throw new ArgumentNullException(nameof(user));
            this.UserId = user.Id;
            this.IsPublic = true;
        }

        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public Guid UploadId { get; set; }
        public long FileSize { get; set; }
        public string FileExt { get; set; }
        public string Picture { get; set; }
        public bool IsPublic { get; set; }
        public bool IsLoop { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public long? GenreId { get; set; }
        public Genre Genre { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public ICollection<FavoriteAudio> Favorited { get; set; }

        public void UpdateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                this.Title = title;
            }
        }

        public void UpdateDescription(string description)
        {
            if (description is not null)
                this.Description = description;
        }

        public void UpdatePublicStatus(bool? status)
        {
            if (status.HasValue)
                this.IsPublic = status.Value;
        }

        public void UpdateLoop(bool? isLoop)
        {
            if (isLoop.HasValue)
                this.IsLoop = isLoop.Value;
        }

        public void UpdateGenre(Genre genre)
        {
            this.GenreId = genre?.Id;
            this.Genre = genre;
        }

        public void UpdateTags(List<Tag> tags)
        {
            if (this.Tags.Count > 0)
            {
                foreach (var audioTag in this.Tags)
                {
                    if (tags.All(t => t.Id != audioTag.Id))
                    {
                        this.Tags.Remove(audioTag);
                    }
                }

                foreach (var tag in tags)
                {
                    if (this.Tags.All(t => t.Id != tag.Id))
                        this.Tags.Add(tag);
                }
            }
            else
            {
                foreach (var tag in tags)
                {
                    this.Tags.Add(tag);
                }
            }
        }

        public void UpdatePicture(string picturePath)
        {
            if (!string.IsNullOrWhiteSpace(picturePath))
                this.Picture = picturePath;
        }

        public bool CanModify(string userId)
        {
            return this.UserId == userId;
        }

        public bool AddFavorite(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var favorite = GetFavorite(userId);
            
            if (favorite is null)
            {
                favorite = new FavoriteAudio {AudioId = this.Id, UserId = userId};
                this.Favorited.Add(favorite);
            }

            return true;
        }

        public bool RemoveFavorite(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var favorite = GetFavorite(userId);

            if (favorite is not null)
                this.Favorited.Remove(favorite);

            return false;
        }

        private FavoriteAudio GetFavorite(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            return this.Favorited.FirstOrDefault(f => f.UserId == userId);
        }
    }
}
