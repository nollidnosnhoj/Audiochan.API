using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Audio.CreateAudio;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Audiochan.Core.IntegrationTests.Features.Audios
{
    [Collection(nameof(SliceFixture))]
    public class CreateAudioTests
    {
        private readonly SliceFixture _fixture;

        public CreateAudioTests(SliceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Should_Create_New_Audio()
        {
            // ASSIGN
            var userId = await _fixture.RunAsDefaultUserAsync();
            var genre = new Genre {Name = "Dubstep", Slug = "dubstep"};
            await _fixture.InsertAsync(genre);
            var uploadId = Guid.NewGuid();
            
            // ACT
            var result = await _fixture.SendAsync(new CreateAudioCommand
            {
                UploadId = uploadId,
                FileName = "testaudio.mp3",
                Duration = 100,
                FileSize = 10000,
                Title = "Test Audio",
                Description = "This is a test audio",
                Tags = new List<string>{ "apples", "oranges", "banana" },
                Genre = "dubstep",
                IsLoop = true,
                IsPublic = false
            });

            var created = await _fixture.ExecuteDbContextAsync(database =>
            {
                return database.Audios
                    .Include(a => a.Genre)
                    .Include(a => a.Tags)
                    .Where(a => a.Id == result.Data.Id).SingleOrDefaultAsync();
            });

            // ASSERT
            result.IsSuccess.Should().Be(true);
            result.Data.Should().NotBeNull();
            created.Should().NotBeNull();
            created.UploadId.Should().Be(uploadId);
            created.Title.Should().Be("Test Audio");
            created.Description.Should().Be("This is a test audio");
            created.FileExt.Should().Be(".mp3");
            created.Duration.Should().Be(100);
            created.FileSize.Should().Be(10000);
            created.Tags.Count.Should().Be(3);
            created.Tags.Should().Contain(x => x.Id == "apples");
            created.Tags.Should().Contain(x => x.Id == "oranges");
            created.Tags.Should().Contain(x => x.Id == "banana");
            created.Genre.Name.Should().Be("Dubstep");
            created.IsPublic.Should().Be(false);
            created.IsLoop.Should().Be(true);
            created.UserId.Should().Be(userId);
        }
    }
}