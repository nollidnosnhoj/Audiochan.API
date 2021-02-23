﻿using System;
using System.Linq;
using Audiochan.Core.Entities;
using FluentAssertions;
using Xunit;

namespace Audiochan.Core.UnitTests.Entities
{
    public class AudioTests
    {
        public readonly User ValidUser = new("testuser", "testuser@localhost", DateTime.UtcNow)
        {
            Id = "ValidUserId"
        };
        
        [Fact]
        public void NewAudio_ShouldThrow_WhenFileNameIsNullOrEmpty()
        {
            FluentActions.Invoking(() => new Audio(Guid.NewGuid(), null, 0, 0, ValidUser))
                .Should()
                .ThrowExactly<ArgumentNullException>("null", "fileName");
            
            FluentActions.Invoking(() => new Audio(Guid.NewGuid(), string.Empty, 0, 0, ValidUser))
                .Should()
                .ThrowExactly<ArgumentNullException>("empty", "fileName");
            
            FluentActions.Invoking(() => new Audio(Guid.NewGuid(), "  ", 0, 0, ValidUser))
                .Should()
                .ThrowExactly<ArgumentNullException>("whitespace", "fileName");
        }

        [Theory]
        [InlineData("audio.mp3")]
        public void NewAudio_ShouldNotThrow_WhenFilenameDoesHaveExtension(string fileName)
        {
            FluentActions.Invoking(() => new Audio(Guid.NewGuid(), fileName, 0, 0, ValidUser))
                .Should()
                .NotThrow<ArgumentException>();
        }

        [Theory]
        [InlineData("shouldfail")]
        public void NewAudio_ShouldThrow_WhenFilenameDoesNotHaveExtension(string fileName)
        {
            FluentActions.Invoking(() => new Audio(Guid.NewGuid(), fileName, 0, 0, ValidUser))
                .Should()
                .Throw<ArgumentException>("no file extension", "filename");
        }

        [Fact]
        public void NewAudio_ShouldHaveExtension_BasedOnFileName()
        {
            var expectedTitle = "audio";
            var audio = new Audio(Guid.NewGuid(), "audio.mp3", 0, 0, ValidUser);
            audio.Title.Should().Be(expectedTitle);
        }

        [Theory]
        [InlineData("apples", "oranges", "cucumber")]
        public void NewAudio_ShouldHaveCorrectTagValues(params string[] tags)
        {
            // Assign
            var tagEntities = tags.Select(tag => new Tag {Id = tag}).ToList();
            var audio = new Audio();

            // Act
            audio.UpdateTags(tagEntities);
            
            // Assert
            audio.Tags.Count.Should().Be(3);
            audio.Tags.Count(t => t.Id == "apples").Should().Be(1);
        }
        
        [Theory]
        [InlineData("apples", "oranges", "cucumber")]
        public void UpdateAudio_ShouldHaveCorrectTagValues(params string[] tags)
        {
            // Assign
            var tagEntities = tags.Select(tag => new Tag {Id = tag}).ToList();
            var audio = new Audio();
            audio.Tags.Add(new Tag{Id = "apples"});

            // Act
            audio.UpdateTags(tagEntities);
            
            // Assert
            audio.Tags.Count.Should().Be(3);
            audio.Tags.Count(t => t.Id == "apples").Should().Be(1);
        }

        [Fact]
        public void AudioShouldHaveFavorited()
        {
            // Assign
            const string currentUserId = "testId";
            var audio = new Audio(Guid.NewGuid(), "filename.mp3", 0, 0, ValidUser);
            
            // Act
            audio.AddFavorite(currentUserId);
            
            // Assert
            audio.Favorited.Any(x => x.UserId == currentUserId).Should().Be(true);
        }

        [Fact]
        public void AudioShouldHaveUnfavorited()
        {
            // Assign
            const string currentUserId = "testId";
            var audio = new Audio(Guid.NewGuid(), "filename.mp3", 0, 0, ValidUser);
            audio.Favorited.Add(new FavoriteAudio {AudioId = audio.Id, UserId = currentUserId});
            
            // Act
            audio.RemoveFavorite(currentUserId);
            
            // Assert
            audio.Favorited.Any(x => x.UserId == currentUserId).Should().Be(false);
        }
    }
}