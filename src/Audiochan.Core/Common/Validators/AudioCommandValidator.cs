﻿using Audiochan.Core.Common.Models;
using FluentValidation;

namespace Audiochan.Core.Common.Validators
{
    public class AudioCommandValidator : AbstractValidator<AudioCommand>
    {
        public AudioCommandValidator()
        {
            // When the title is present, it must be at most 30 characters long.
            RuleFor(req => req.Title)
                .MaximumLength(30)
                .When(req => !string.IsNullOrWhiteSpace(req.Title))
                .WithMessage("Title cannot be no more than 30 characters long.");

            // Description must be at most 500 characters long.
            RuleFor(req => req.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot be more than 500 characters long.");
            
            // Must be at most 10 tags.
            RuleFor(req => req.Tags)
                .Must(u => u!.Count <= 10)
                .WithMessage("Can only have up to 10 tags per audio upload.");
        }
    }
}