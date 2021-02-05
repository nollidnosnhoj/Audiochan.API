﻿using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Options;
using Audiochan.Core.Features.Auth.Models;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Audiochan.Core.Features.Auth.Validators
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator(IOptions<IdentityOptions> options)
        {
            RuleFor(req => req.Username).Username(options.Value);
            RuleFor(req => req.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is invalid.");
            RuleFor(req => req.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Password(options.Value);
        }
    }
}