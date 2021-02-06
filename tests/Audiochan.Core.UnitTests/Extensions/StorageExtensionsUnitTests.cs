using System.IO;
using Audiochan.Core.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Audiochan.Core.UnitTests.Extensions
{
    public class StorageExtensionsUnitTests
    {
        [Fact]
        public void GetValidContentTypeTest()
        {
            var contentType = "abc123.mp3".GetContentType();
            contentType.Should().Be("audio/mpeg");
        }

        [Fact]
        public void GetInvalidContentTypeTest()
        {
            var contentType = "abc123".GetContentType();
            contentType.Should().Be("application/octet-stream");
        }
        
        [Fact]
        public void GetBlobPathTest()
        {
            var path = Path.Combine("audios", "some_id", "source.mp3");
            var (container, blobName) = path.GetBlobPath();
            container.Should().Be("audios\\some_id");
            blobName.Should().Be("source.mp3");
        }

        [Fact]
        public void GetKeyNameWithoutExtensionParameterTest()
        {
            string container = "audios";
            string blobName = Path.Combine("abc123", "source.mp3");
            var key = blobName.GetKeyName(container);

            key.Should().Be("audios/abc123/source.mp3");
        }

        [Fact]
        public void GetKeyNameWithExtensionParameterTest()
        {
            string container = "audios";
            string blobName = Path.Combine("abc123", "source");
            string fileName = ".mp3";
            var key = blobName.GetKeyName(container, fileName);
            key.Should().Be("audios/abc123/source.mp3");
        }
    }
}