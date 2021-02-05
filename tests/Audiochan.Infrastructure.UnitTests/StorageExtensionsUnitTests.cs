using Audiochan.Infrastructure.Storage.Extensions;
using FluentAssertions;
using Xunit;

namespace Audiochan.Infrastructure.UnitTests
{
    public class StorageExtensionsUnitTests
    {
        [Fact]
        public void GetBlobPathTest()
        {
            var storageUrl = "http://localhost:5000/uploads";
            var url = "http://localhost:5000/uploads/pictures/audios/abc123.jpg";
            var (container, blobName) = url.GetBlobPath(storageUrl);
            container.Should().Be("pictures/audios");
            blobName.Should().Be("abc123.jpg");
        }
    }
}