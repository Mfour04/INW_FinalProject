using Application.Features.Novel.Commands;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Novel.Commands
{
    public class UpdateLockNovelHandlerTests
    {
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly UpdateLockNovelHandler _handler;

        public UpdateLockNovelHandlerTests()
        {
            _novelRepoMock = new Mock<INovelRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _handler = new UpdateLockNovelHandler(_novelRepoMock.Object, _currentUserMock.Object);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_UserId_Is_Empty()
        {
            _currentUserMock.Setup(c => c.UserId).Returns(string.Empty);

            var command = new UpdateLockNovelCommand { NovelId = "n1", IsLocked = true };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Unauthorized");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Is_Not_Admin()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("u1");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(false);

            var command = new UpdateLockNovelCommand { NovelId = "n1", IsLocked = true };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Forbidden: Admin role required");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Not_Found()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("admin");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(true);

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync((NovelEntity?)null);

            var command = new UpdateLockNovelCommand { NovelId = "n1", IsLocked = true };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Novel not found");
        }

        [Fact]
        public async Task Handle_Should_Update_Lock_Status_Successfully()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("admin");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(true);

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1" });

            var command = new UpdateLockNovelCommand { NovelId = "n1", IsLocked = true };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("locked successfully");
            _novelRepoMock.Verify(n => n.UpdateLockStatusAsync("n1", true), Times.Once);
        }
    }
}
