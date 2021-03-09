﻿using System;
using Audiochan.Core.Common.Models;

namespace Audiochan.Core.Features.Audios.GetAudio
{
    public record AudioViewModel
    {
        public long Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public bool IsPublic { get; init; }
        public string[] Tags { get; init; }
        public int Duration { get; init; }
        public long FileSize { get; init; }
        public string FileExt { get; init; }
        public string Picture { get; init; }
        public int FavoriteCount { get; init; }
        public bool IsFavorited { get; init; }
        public DateTime Created { get; init; }
        public DateTime? LastModified { get; init; }
        public GenreDto Genre { get; init; }

        public UserDto User { get; init; }

        // TODO: I want to remove UploadId eventually from the view model.
        public string UploadId { get; init; }

        public record GenreDto(long Id, string Name, string Slug)
        {
        }
    }
}