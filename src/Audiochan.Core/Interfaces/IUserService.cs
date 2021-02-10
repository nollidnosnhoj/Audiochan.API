﻿using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Models.Result;
using Audiochan.Core.Features.Users.Models;
using Microsoft.AspNetCore.Http;

namespace Audiochan.Core.Interfaces
{
    public interface IUserService
    {
        Task<IResult<CurrentUserViewModel>> GetCurrentUser(string authUserId, CancellationToken cancellationToken = default);
        Task<IResult<ProfileViewModel>> GetUserProfile(string username, CancellationToken cancellationToken = default);
        Task<IResult<string>> AddPicture(string userId, string data, CancellationToken cancellationToken = default);
        Task<IResult> UpdateUsername(string userId, string newUsername, CancellationToken cancellationToken = default);
        Task<IResult> UpdateEmail(string userId, string newEmail, CancellationToken cancellationToken = default);
        Task<IResult> UpdatePassword(string userId, ChangePasswordRequest request, 
            CancellationToken cancellationToken = default);
        Task<IResult> UpdateUser(string userId, UpdateUserDetailsRequest request, CancellationToken cancellationToken = default);
        Task<bool> CheckIfUsernameExists(string username, CancellationToken cancellationToken = default);
        Task<bool> CheckIfEmailExists(string email, CancellationToken cancellationToken = default);
    }
}