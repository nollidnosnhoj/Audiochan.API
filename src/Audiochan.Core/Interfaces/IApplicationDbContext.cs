﻿using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Audio> Audios { get; }
        DbSet<FavoriteAudio> FavoriteAudios { get; }
        DbSet<FollowedUser> FollowedUsers { get; }
        DbSet<Genre> Genres { get; }
        DbSet<Tag> Tags { get; }
        DbSet<User> Users { get; }
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}