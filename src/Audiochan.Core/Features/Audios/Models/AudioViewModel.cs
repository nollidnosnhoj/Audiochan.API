﻿using System;
using Audiochan.Core.Features.Genres.Models;
using Audiochan.Core.Features.Users.Models;

namespace Audiochan.Core.Features.Audios.Models
{
    public record AudioViewModel
    {
        public long Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public bool IsPublic { get; init; }
        public bool IsLoop { get; init; }
        public string Tags { get; init; }
        public int Duration { get; init; }
        public long FileSize { get; init; }
        public string FileExt { get; init; }
        public string PictureUrl { get; init; }
        public int FavoriteCount { get; init; }
        public bool IsFavorited { get; init; }
        public DateTime Created { get; init; }
        public DateTime? Updated { get; init; }
        public string Genre { get; init; }
        public UserViewModel User { get; init; }
        public Guid UploadId { get; init; }
    }
}