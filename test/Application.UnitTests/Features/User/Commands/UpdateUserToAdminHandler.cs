using Application.Features.User.Feature;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.User.Commands
{
    public class UpdateUserToAdminHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly UpdateUserToAdminHandler _handler;

        public UpdateUserToAdminHandlerTests()
        {
            _handler = new UpdateUserToAdminHandler(_userRepoMock.Object);
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Fail_When_User_NotFound()
        {
            // Arrange
            var command = new UpdateUserToAdminCommand { UserId = "user1" };

            _userRepoMock.Setup(r => r.GetById("user1"))
                .ReturnsAsync((UserEntity?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User Not Found");
        }

        // ---------- HAPPY PATH ----------

        [Fact]
        public async Task Handle_Should_Succeed_When_User_Updated_To_Admin()
        {
            // Arrange
            var command = new UpdateUserToAdminCommand { UserId = "user1" };

            var existingUser = new UserEntity
            {
                id = "user1",
                username = "user1",
                role = Role.User
            };

            _userRepoMock.Setup(r => r.GetById("user1"))
                .ReturnsAsync(existingUser);

            _userRepoMock.Setup(r => r.UpdateUserRoleToAdminAsync("user1"))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Update to admin successfully");
        }
    }
}
