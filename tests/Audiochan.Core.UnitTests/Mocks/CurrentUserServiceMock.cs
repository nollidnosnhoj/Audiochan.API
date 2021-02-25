using Audiochan.Core.Interfaces;
using Moq;

namespace Audiochan.Core.UnitTests.Mocks
{
    public static class CurrentUserServiceMock
    {
        public static Mock<ICurrentUserService> Create(string userId = "00000000-0000-0000-0000-000000000000")
        {
            var mock = new Mock<ICurrentUserService>();
            mock.Setup(x => x.GetUserId()).Returns(userId);
            return mock;
        }
    }
}