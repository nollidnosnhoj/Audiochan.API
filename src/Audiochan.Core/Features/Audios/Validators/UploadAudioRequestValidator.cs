using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Options;
using Audiochan.Core.Features.Audios.Models;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Audiochan.Core.Features.Audios.Validators
{
    public class UploadAudioRequestValidator : AbstractValidator<UploadAudioRequest>
    {
        public UploadAudioRequestValidator(IOptions<AudiochanOptions> options)
        {
            var uploadOptions = options.Value.AudioUploadOptions;

            When(req => req.File is not null, () =>
            {
                RuleFor(req => req.File)
                    .FileValidation(uploadOptions.ContentTypes, uploadOptions.FileSize);
            });

            When(req => req.File is null, () =>
            {
                RuleFor(req => req.Id)
                    .NotEmpty()
                    .WithMessage("Id is required.");
                RuleFor(req => req.Duration)
                    .NotEmpty()
                    .WithMessage("Duration is required.");
                RuleFor(req => req.FileName)
                    .NotEmpty()
                    .WithMessage("Filename is required.");
            });
            
            Include(new UpdateAudioRequestValidator());
        }
    }
}