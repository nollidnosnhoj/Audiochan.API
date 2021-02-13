﻿using System.Collections.Generic;
using Audiochan.Core.Common.Options;
using Audiochan.Core.Features.Audios.Models;
using Audiochan.Core.Features.Audios.Validators;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using Xunit;

namespace Audiochan.Core.UnitTests.Validations
{
    public class UploadAudioValidationTests
    {
        private readonly IValidator<CreateAudioRequest> _validator;

        public UploadAudioValidationTests()
        {
            var options = Options.Create(new AudiochanOptions
            {
                AudioUploadOptions = new AudiochanOptions.UploadOptions
                {
                    ContentTypes = new List<string>
                    {
                        "audio/mp3",
                        "audio/mpeg",
                        "audio/ogg"
                    },
                    FileSize = 262144000
                }
            });
            _validator = new UploadAudioRequestValidator(options);
        }
        
        [Fact]
        public void CheckIfTitleIsValidWhenEmpty()
        {
            var result = _validator.TestValidate(new CreateAudioRequest{Title=""});
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void CheckIfOverTenTagsIsInvalid()
        {
            var tags = new List<string?>
            {
                "word1", "word2", "word3", "word4", "word5", "word6", "word7", "word8", "word9", "word10", "word11"
            };

            var dto = new CreateAudioRequest {Tags = tags};
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Tags);
        }
        
        [Fact]
        public void CheckIfLessOrEqualToTenTagsIsValid()
        {
            var tags = new List<string?>
            {
                "word1", "word2", "word3", "word4", "word5", "word6", "word7", "word8", "word9"
            };

            var dto = new CreateAudioRequest {Tags = tags};
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Tags);
        }
        
        [Fact]
        public void CheckIfNullTagsIsValid()
        {
            var dto = new CreateAudioRequest();
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Tags);
        }
        
        [Fact]
        public void CheckIfEmptyTagsIsValid()
        {
            var dto = new CreateAudioRequest{Tags = new List<string?>()};
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Tags);
        }
    }
}