using System;
using System.Linq;
using System.Linq.Expressions;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Users.GetProfile;

namespace Audiochan.Core.Common.Mappings
{
    public static class UserMappings
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
                IsFollowing = !string.IsNullOrEmpty(currentUserId)
                    ? user.Followers.Any(f => f.ObserverId == currentUserId)
                    : null
            };
        }
    }
}