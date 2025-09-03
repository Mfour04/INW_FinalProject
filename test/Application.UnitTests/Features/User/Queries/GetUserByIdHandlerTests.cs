using Application.Features.User.Queries;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shared.Contracts.Response.Tag;
using Shared.Contracts.Response.User;
using Shouldly;

namespace Application.UnitTests.Features.User.Queries
{
    public class GetUserByIdHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<ITagRepository> _tagRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
        private readonly GetUserByIdHanlder _handler;

        public GetUserByIdHandlerTests()
        {
            _handler = new GetUserByIdHanlder(
                _userRepoMock.Object,
                _mapperMock.Object,
                _currentUserServiceMock.Object,
                _tagRepoMock.Object
            );
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Return_Fail_When_User_Not_Found()
        {
            var command = new GetUserById { UserId = "u1", CurrentUserId = "u1" };

            _userRepoMock.Setup(r => r.GetById("u1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _handler.Handle(command, default);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User not found");
        }

        [Fact]
        public async Task Handle_Should_Return_Fail_When_Exception_Occurs()
        {
            var command = new GetUserById { UserId = "u1", CurrentUserId = "u1" };

            _userRepoMock.Setup(r => r.GetById("u1"))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _handler.Handle(command, default);

            result.Success.ShouldBeFalse();
            result.Message.ShouldContain("An error occurred");
        }

        // ---------- HAPPY PATH ----------

        [Fact]
        public async Task Handle_Should_Return_User_With_All_Info_When_Self()
        {
            var command = new GetUserById { UserId = "u1", CurrentUserId = "u1" };

            var user = new UserEntity
            {
                id = "u1",
                email = "user@email.com",
                coin = 100,
                block_coin = 50,
                favourite_type = new List<string> { "tag-1" }
            };

            var tagList = new List<TagEntity>
            {
                new TagEntity { id = "tag-1", name = "Fantasy" }
            };

            var expectedResponse = new UserResponse
            {
                UserId = "u1",
                Email = "user@email.com",
                Coin = 100,
                BlockCoin = 50,
                FavouriteType = new List<TagListResponse>()
            };

            _userRepoMock.Setup(r => r.GetById("u1")).ReturnsAsync(user);
            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(tagList);
            _mapperMock.Setup(m => m.Map<UserResponse>(user)).Returns(expectedResponse);
            _currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(false);

            var result = await _handler.Handle(command, default);

            result.Success.ShouldBeTrue();
            var res = result.Data as UserResponse;
            res.ShouldNotBeNull();
            res.Email.ShouldBe("user@email.com");
            res.Coin.ShouldBe(100);
        }

        [Fact]
        public async Task Handle_Should_Hide_Info_When_Not_Self_And_Not_Admin()
        {
            var command = new GetUserById { UserId = "u1", CurrentUserId = "other-user" };

            var user = new UserEntity
            {
                id = "u1",
                email = "user@email.com",
                coin = 100,
                block_coin = 50,
                favourite_type = new List<string>()
            };

            var expectedResponse = new UserResponse
            {
                UserId = "u1",
                Email = "user@email.com",
                Coin = 100,
                BlockCoin = 50,
                FavouriteType = new List<TagListResponse>()
            };

            _userRepoMock.Setup(r => r.GetById("u1")).ReturnsAsync(user);
            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity>());
            _mapperMock.Setup(m => m.Map<UserResponse>(user)).Returns(expectedResponse);
            _currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(false);

            var result = await _handler.Handle(command, default);

            result.Success.ShouldBeTrue();
            var res = result.Data as UserResponse;
            res.ShouldNotBeNull();
            res.Email.ShouldBeNull();
            res.Coin.ShouldBe(0);
            res.BlockCoin.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_Should_Show_Full_Info_When_Admin()
        {
            var command = new GetUserById { UserId = "u1", CurrentUserId = "admin-id" };

            var user = new UserEntity
            {
                id = "u1",
                email = "admin@inkwave.com",
                coin = 999,
                block_coin = 0,
                favourite_type = new List<string>()
            };

            var expectedResponse = new UserResponse
            {
                UserId = "u1",
                Email = "admin@inkwave.com",
                Coin = 999,
                BlockCoin = 0,
                FavouriteType = new List<TagListResponse>()
            };

            _userRepoMock.Setup(r => r.GetById("u1")).ReturnsAsync(user);
            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity>());
            _mapperMock.Setup(m => m.Map<UserResponse>(user)).Returns(expectedResponse);
            _currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);

            var result = await _handler.Handle(command, default);

            result.Success.ShouldBeTrue();
            var res = result.Data as UserResponse;
            res.ShouldNotBeNull();
            res.Email.ShouldBe("admin@inkwave.com");
            res.Coin.ShouldBe(999);
        }
    }
}
