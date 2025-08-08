using Application.Features.User.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shared.Contracts.Response.User;
using Shouldly;

namespace Application.UnitTests.Features.User.Queries
{
    public class GetAdminIdHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetAdminIdHandler _handler;

        public GetAdminIdHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetAdminIdHandler(_userRepoMock.Object, _mapperMock.Object);
        }

        // ---------- HAPPY PATH ----------

        [Fact]
        public async Task Handle_Should_Return_Admin_User_When_Found()
        {
            // Arrange
            var adminUser = new UserEntity
            {
                id = "admin1",
                username = "admin@example.com",
                role = Role.Admin
            };

            var expectedResponse = new UserResponse
            {
                UserId = "admin1",
                UserName = "admin@example.com"
            };

            _userRepoMock.Setup(repo => repo.GetFirstUserByRoleAsync(Role.Admin))
                .ReturnsAsync(adminUser);

            _mapperMock.Setup(m => m.Map<UserResponse>(adminUser))
                .Returns(expectedResponse);

            // Act
            var result = await _handler.Handle(new GetAdminId(), default);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("Admin user retrieved successfully.");
            result.Data.ShouldBeOfType<UserResponse>();
            ((UserResponse)result.Data).UserId.ShouldBe("admin1");
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Return_Fail_When_Admin_Not_Found()
        {
            // Arrange
            _userRepoMock.Setup(repo => repo.GetFirstUserByRoleAsync(Role.Admin))
                .ReturnsAsync((UserEntity)null!);
            // Act
            var result = await _handler.Handle(new GetAdminId(), default);

            // Assert
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Admin user not found.");
        }
    }
}
