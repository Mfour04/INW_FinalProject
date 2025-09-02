using Application.Features.Notification.Commands;
using Application.Features.User.Feature;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Moq;
using Shared.Contracts.Response;
using Shouldly;

namespace Application.UnitTests.Features.User.Commands
{
    public class UpdateLockvsUnLockUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<ICurrentUserService> _currentUserMock = new();
        private readonly Mock<INotificationRepository> _notificationRepoMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();

        private readonly UpdateLockvsUnLockUserHandler _handler;

        public UpdateLockvsUnLockUserHandlerTests()
        {
            _handler = new UpdateLockvsUnLockUserHandler(
                _userRepoMock.Object,
                _currentUserMock.Object,
                _notificationRepoMock.Object,
                _notificationServiceMock.Object,
                _mediatorMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_If_User_Is_Not_Admin()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("user123");
            _currentUserMock.Setup(c => c.Role).Returns("User");

            var command = new UpdateLockvsUnLockUserCommand
            {
                UserIds = new List<string> { "u1" },
                isBanned = true,
                DurationType = "3 ngày"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Forbidden: Only admins can perform this action.");
        }

        [Fact]
        public async Task Handle_Should_Fail_With_Invalid_Duration()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("admin1");
            _currentUserMock.Setup(c => c.Role).Returns("Admin");

            var command = new UpdateLockvsUnLockUserCommand
            {
                UserIds = new List<string> { "u1" },
                isBanned = true,
                DurationType = "999 ngày"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Invalid ban duration type.");
        }

        [Fact]
        public async Task Handle_Should_Ban_User_With_Valid_Duration()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("admin1");
            _currentUserMock.Setup(c => c.Role).Returns("Admin");

            var user = new UserEntity { id = "u1", username = "testuser" };

            _userRepoMock.Setup(r => r.GetById("u1")).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.UpdateLockvsUnLockUser("u1", true, It.IsAny<long?>()))
                .Returns(Task.CompletedTask);

            _mediatorMock.Setup(m => m.Send(It.IsAny<SendNotificationToUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse { Success = true, Data = new { SignalRSent = true } });

            var command = new UpdateLockvsUnLockUserCommand
            {
                UserIds = new List<string> { "u1" },
                isBanned = true,
                DurationType = "3 ngày"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("Ban 1 users until 72 hours");

            _userRepoMock.Verify(r => r.UpdateLockvsUnLockUser("u1", true, It.IsAny<long?>()), Times.Once);
            _mediatorMock.Verify(m => m.Send(It.IsAny<SendNotificationToUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Ban_User_Permanently_When_Duration_Is_Forever()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("admin1");
            _currentUserMock.Setup(c => c.Role).Returns("Admin");

            var user = new UserEntity { id = "u1" };

            _userRepoMock.Setup(r => r.GetById("u1")).ReturnsAsync(user);

            _userRepoMock.Setup(r => r.UpdateLockvsUnLockUser("u1", true, It.IsAny<long?>()))
                .Returns(Task.CompletedTask);

            _mediatorMock.Setup(m => m.Send(It.IsAny<SendNotificationToUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse { Success = true, Data = new { SignalRSent = true } });

            var command = new UpdateLockvsUnLockUserCommand
            {
                UserIds = new List<string> { "u1" },
                isBanned = true,
                DurationType = "Vĩnh viễn"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("permanent");
            _userRepoMock.Verify(r => r.UpdateLockvsUnLockUser("u1", true, It.IsAny<long?>()), Times.Once);
        }


        [Fact]
        public async Task Handle_Should_Unban_User()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("admin1");
            _currentUserMock.Setup(c => c.Role).Returns("Admin");

            var user = new UserEntity { id = "u1" };

            _userRepoMock.Setup(r => r.GetById("u1")).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.UpdateLockvsUnLockUser("u1", false, null))
                .Returns(Task.CompletedTask);

            _mediatorMock.Setup(m => m.Send(It.IsAny<SendNotificationToUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse { Success = true, Data = new { SignalRSent = true } });

            var command = new UpdateLockvsUnLockUserCommand
            {
                UserIds = new List<string> { "u1" },
                isBanned = false
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("Unban 1 users");
            _userRepoMock.Verify(r => r.UpdateLockvsUnLockUser("u1", false, null), Times.Once);
        }
    }
}
