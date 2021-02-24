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

        public CreateAudioTests(SliceFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Should_Create_New_Audio()
        {
            // ASSIGN
            var userId = await _fixture.RunAsDefaultUserAsync();
            var genre = new Genre {Name = "Dubstep", Slug = "dubstep"};
            await _fixture.InsertAsync(genre);
            
            // ACT
            var result = await _fixture.SendAsync(new CreateAudioCommand
            {
                UploadId = Guid.NewGuid(),
                FileName = "testaudio.mp3",
                Duration = 100,
                FileSize = 10000,
                Tags = new List<string>{ "apples", "oranges", "banana" },
                Genre = "dubstep"
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
            created.Title.Should().Be("testaudio");
            created.FileExt.Should().Be(".mp3");
            created.Tags.Count.Should().Be(3);
            created.Tags.Should().Contain(x => x.Id == "apples");
            created.Tags.Should().Contain(x => x.Id == "oranges");
            created.Tags.Should().Contain(x => x.Id == "banana");
            created.Genre.Name.Should().Be("Dubstep");
            created.UserId.Should().Be(userId);
        }
    }
}