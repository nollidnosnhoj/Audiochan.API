using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Audiochan.Core.Features.Users.UpdatePicture
{
    public record UpdateUserPictureCommand(string UserId, string ImageData) : IRequest<IResult<string>>
    {
        
    }
    
    public class UpdateUserPictureCommandHandler : IRequestHandler<UpdateUserPictureCommand, IResult<string>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IImageService _imageService;
        private readonly IStorageService _storageService;

        public UpdateUserPictureCommandHandler(UserManager<User> userManager,
            IImageService imageService, 
            IStorageService storageService)
        {
            _userManager = userManager;
            _imageService = imageService;
            _storageService = storageService;
        }
        
        public async Task<IResult<string>> Handle(UpdateUserPictureCommand request, CancellationToken cancellationToken)
        {
            var blobName = Path.Combine(request.UserId, Guid.NewGuid().ToString("N") + ".jpg");
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId + "");
                if (user == null) return Result<string>.Fail(ResultError.Unauthorized);
                if (!string.IsNullOrEmpty(user.Picture))
                {
                    await _storageService.RemoveAsync(user.Picture, cancellationToken);
                    user.Picture = string.Empty;
                }
                var response = await _imageService.UploadImage(request.ImageData, PictureType.User, blobName, cancellationToken);
                user.Picture = response.Path;
                await _userManager.UpdateAsync(user);
                return Result<string>.Success(response.Url);
            }
            catch (Exception)
            {
                var task2 = _imageService.RemoveImage(PictureType.User, blobName, cancellationToken);
                await Task.WhenAll(task2);
                throw;
            }
        }
    }
}