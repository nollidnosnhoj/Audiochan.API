using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Entities;
using Audiochan.Core.Features.Audio.CreateAudio;
using Audiochan.Core.Features.Audio.GetAudioList;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Audiochan.Core.IntegrationTests.Features.Audios
{
    [Collection(nameof(SliceFixture))]
    public class GetAudiosTests
    {
        private readonly SliceFixture _fixture;

        public GetAudiosTests(SliceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldGetAudios_BasedOnGenre()
        {
            const int resultCount = 2;
            
            var genreName = Guid.NewGuid().ToString("N");
            var genre = new Genre {Name = genreName, Slug = genreName.GenerateSlug()};
            await _fixture.InsertAsync(genre);

            await _fixture.RunAsDefaultUserAsync();
            for (var i = 0; i < 10; i++)
            {
                await _fixture.SendAsync(new CreateAudioCommand
                {
                    UploadId = Guid.NewGuid(),
                    FileName = Guid.NewGuid().ToString("N") + ".mp3",
                    Duration = 100,
                    FileSize = 100,
                    Genre = (i < resultCount) ? genre.Slug : ""
                });
            }

            var result = await _fixture.SendAsync(new GetAudioListQuery
            {
                Genre = genre.Slug
            });

            result.Should().NotBeNull();
            result.Count.Should().Be(resultCount);
            result.Items.Count.Should().Be(resultCount);
        }

        [Fact]
        public async Task ShouldGetAudios_BasedOnUsername()
        {
            const int resultCount = 3;
            
            var userId = await _fixture
                .RunAsUserAsync("kopacetic", Guid.NewGuid().ToString(), Array.Empty<string>());
            
            var username = await _fixture.ExecuteDbContextAsync(db =>
            {
                return db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.UserName)
                    .SingleOrDefaultAsync();
            });

            for (var i = 0; i < 10; i++)
            {
                if (i >= resultCount)
                {
                    await _fixture.RunAsDefaultUserAsync();
                }

                await _fixture.SendAsync(new CreateAudioCommand
                {
                    UploadId = Guid.NewGuid(),
                    FileName = "test.mp3",
                    Duration = 100,
                    FileSize = 100
                });
            }

            var result = await _fixture.SendAsync(new GetAudioListQuery
            {
                Username = username
            });
            
            result.Should().NotBeNull();
            result.Count.Should().Be(resultCount);
            result.Items.Count.Should().Be(resultCount);
        }

        [Fact]
        public async Task ShouldGetAudios_BasedOnTags()
        {
            const int resultCount = 6;
            const int resultApplesCount = 4;
            const int resultOrangesCount = 3;

            await _fixture.RunAsDefaultUserAsync();

            for (var i = 0; i < 10; i++)
            {
                var tags = new List<string>();
                
                if (i > 0 && i % 2 == 0)
                    tags.Add("apples");
                if (i > 0 && i % 3 == 0)
                    tags.Add("oranges");

                await _fixture.SendAsync(new CreateAudioCommand
                {
                    UploadId = Guid.NewGuid(),
                    FileName = "test.mp3",
                    Duration = 100,
                    FileSize = 100,
                    Tags = tags
                });
            }

            var result = await _fixture.SendAsync(new GetAudioListQuery
            {
                Tags = "apples,oranges"
            });

            result.Should().NotBeNull();
            result.Count.Should().Be(resultCount);
            result.Items.Count.Should().Be(resultCount);
        }
    }
}