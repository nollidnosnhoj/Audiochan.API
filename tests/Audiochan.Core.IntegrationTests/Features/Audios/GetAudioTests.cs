using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Audio.GetAudio;
using Audiochan.Core.UnitTests.Builders;
using FluentAssertions;
using Xunit;

namespace Audiochan.Core.IntegrationTests.Features.Audios
{
    [Collection(nameof(SliceFixture))]
    public class GetAudioTests
    {
        private readonly SliceFixture _fixture;

        public GetAudioTests(SliceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldNotGetAudio_WhenAudioIdIsInvalid()
        {
            // Assign
            var ownerId = await _fixture.RunAsDefaultUserAsync();
            var audio = new AudioBuilder("myaudio.mp3", ownerId).Build();
            await _fixture.InsertAsync(audio);
            
            // Act
            var result = await _fixture.SendAsync(new GetAudioQuery(0));
            
            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().Be(false);
            result.ErrorCode.Should().Be(ResultError.NotFound);
        }

        [Fact]
        public async Task ShouldNotGetAudio_WhenAudioIsPrivateAndUserIsNotTheOwner()
        {
            // Assign
            var adminId = await _fixture.RunAsAdministratorAsync();
            var audio = new AudioBuilder(Guid.NewGuid() + ".mp3", adminId)
                .Public(false)
                .Build();
            await _fixture.InsertAsync(audio);
            
            // Act
            await _fixture.RunAsDefaultUserAsync();
            var result = await _fixture.SendAsync(new GetAudioQuery(audio.Id));
            
            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().Be(false);
            result.ErrorCode.Should().Be(ResultError.NotFound);
        }
        
        [Fact]
        public async Task ShouldGetAudio_WhenAudioIsPrivateAndUserIsTheOwner()
        {
            // Assign
            var adminId = await _fixture.RunAsAdministratorAsync();
            var audio = new AudioBuilder(Guid.NewGuid() + ".mp3", adminId)
                .Public(false)
                .Build();
            await _fixture.InsertAsync(audio);
            
            // Act
            var result = await _fixture.SendAsync(new GetAudioQuery(audio.Id));
            
            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().Be(true);
            result.Data.Should().NotBeNull();
            result.Data.Should().BeOfType<AudioViewModel>();
        }

        [Fact]
        public async Task ShouldGetAudio()
        {
            // Assign
            var ownerId = await _fixture.RunAsDefaultUserAsync();

            var genre = new Genre {Name = "Dubstep", Slug = "dubstep"};

            var tags = new List<Tag>
            {
                new() {Id = "apples"},
                new() {Id = "oranges"}
            };

            var audio = new AudioBuilder("myaudio.mp3", ownerId)
                .Title("Test Audio")
                .Description("This is a test audio")
                .Genre(genre)
                .Tags(tags)
                .Build();
            
            await _fixture.InsertAsync(audio);
            
            // Act
            var result = await _fixture.SendAsync(new GetAudioQuery(audio.Id));
            
            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().Be(true);
            result.Data.Should().NotBeNull();
            result.Data.Should().BeOfType<AudioViewModel>();
            result.Data.Should().NotBeNull();
            result.Data.Title.Should().Be(audio.Title);
            result.Data.Description.Should().Be(audio.Description);
            result.Data.Genre.Should().NotBeNull();
            result.Data.Genre.Slug.Should().Be(audio.Genre.Slug);
            result.Data.Tags.Length.Should().Be(2);
            result.Data.Tags.Should().Contain(x => x == "apples");
            result.Data.Tags.Should().Contain(x => x == "oranges");
            result.Data.IsLoop.Should().Be(audio.IsLoop);
        }
    }
}