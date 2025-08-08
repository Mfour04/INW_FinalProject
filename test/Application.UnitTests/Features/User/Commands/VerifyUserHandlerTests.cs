using Application.Features.User.Feature;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.User.Commands
{
    public class VerifyUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IBadgeProgressService> _badgeServiceMock = new();
        private readonly VerifyUserHandler _handler;

        public VerifyUserHandlerTests()
        {
            _handler = new VerifyUserHandler(
                _userRepoMock.Object,
                _badgeServiceMock.Object
            );
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Found()
        {
            // Arrange
            var command = new VerifyUserCommand { UserId = "user1" };

            _userRepoMock.Setup(r => r.GetById("user1"))
                .ReturnsAsync((UserEntity?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Already_Verified()
        {
            // Arrange
            var command = new VerifyUserCommand { UserId = "user1" };

            var user = new UserEntity { id = "user1", is_verified = true };

            _userRepoMock.Setup(r => r.GetById("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User already verified.");
        }

        // ---------- HAPPY PATH ----------

        [Fact]
        public async Task Handle_Should_Succeed_When_User_Verified_Successfully()
        {
            // Arrange
            var command = new VerifyUserCommand { UserId = "user1" };

            var user = new UserEntity
            {
                id = "user1",
                is_verified = false
            };

            _userRepoMock.Setup(r => r.GetById("user1"))
                .ReturnsAsync(user);

            _badgeServiceMock.Setup(b => b.InitializeUserBadgeProgress("user1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Email Verified Successfully");

            _userRepoMock.Verify(r => r.UpdateUser(It.Is<UserEntity>(u => u.is_verified == true)), Times.Once);
            _badgeServiceMock.Verify(b => b.InitializeUserBadgeProgress("user1"), Times.Once);
        }
    }
}
