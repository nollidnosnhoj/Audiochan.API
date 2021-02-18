﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Enums;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Audio.UpdatePicture
{
    public record UpdateAudioPictureCommand(long Id, string ImageData) : IRequest<IResult<string>>
    {
        
    }

    public class UpdateAudioPictureCommandHandler : IRequestHandler<UpdateAudioPictureCommand, IResult<string>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImageService _imageService;

        public UpdateAudioPictureCommandHandler(IApplicationDbContext dbContext, IStorageService storageService, ICurrentUserService currentUserService, IImageService imageService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
            _currentUserService = currentUserService;
            _imageService = imageService;
        }
        
        public async Task<IResult<string>> Handle(UpdateAudioPictureCommand request, CancellationToken cancellationToken)
        {
            var blobName = request.Id + "_" + Guid.NewGuid().ToString("N") + ".jpg";
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var currentUserId = _currentUserService.GetUserId();

                var audio = await _dbContext.Audios
                    .AsNoTracking()
                    .SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

                if (audio == null) return Result<string>.Fail(ResultStatus.NotFound);
                if (audio.UserId != currentUserId) return Result<string>.Fail(ResultStatus.Forbidden);
                if (!string.IsNullOrEmpty(audio.Picture))
                {
                    await _storageService.RemoveAsync(audio.Picture, cancellationToken);
                    audio.Picture = string.Empty;
                }
                
                var response = await _imageService.UploadImage(request.ImageData, PictureType.Audio, blobName, cancellationToken);
                audio.Picture = response.Path;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result<string>.Success(response.Url);
            }
            catch (Exception)
            {
                var task1 = transaction.RollbackAsync(cancellationToken);
                var task2 = _imageService.RemoveImage(PictureType.Audio, blobName, cancellationToken);
                await Task.WhenAll(task1, task2);
                throw;
            }
        }
    }
}