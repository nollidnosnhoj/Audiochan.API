using System;
using System.Linq;
using System.Linq.Expressions;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Users.Models;

namespace Audiochan.Core.Features.Users.Mappings
{
    public static class UserProfileMapping
    {
        public static Expression<Func<User, ProfileViewModel>> Map(string currentUserId)
        {
            return user => new ProfileViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                AboutMe = user.About,
                Website = user.Website,
                Picture = user.Picture,
                AudioCount = user.Audios.Count,
                FollowerCount = user.Followers.Count,
                FollowingCount = user.Followings.Count,
                IsFollowing = user.Followers.Any(f => f.ObserverId == currentUserId)
            };
        }
    }
}