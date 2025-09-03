using Application.Features.User.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shared.Contracts.Response.Tag;
using Shared.Contracts.Response.User;
using Shouldly;

namespace Application.UnitTests.Features.User.Queries
{
    public class GetAllUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<ITagRepository> _tagRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly GetAllUserHanlder _handler;

        public GetAllUserHandlerTests()
        {
            _handler = new GetAllUserHanlder(
                _userRepoMock.Object,
                _mapperMock.Object,
                _tagRepoMock.Object
            );
        }

        // ---------- HAPPY PATH ----------

        [Fact]
        public async Task Handle_Should_Return_Users_With_Tags_When_Found()
        {
            // Arrange
            var users = new List<UserEntity>
            {
                new UserEntity
                {
                    id = "user-1",
                    role = Role.User,
                    favourite_type = new List<string> { "tag-1" }
                }
            };

            var tagList = new List<TagEntity>
            {
                new TagEntity { id = "tag-1", name = "Fantasy" }
            };

            _userRepoMock.Setup(r => r.GetAllUserAsync(It.IsAny<FindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync((users, users.Count));

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(tagList);

            _mapperMock.Setup(m => m.Map<List<UserResponse>>(users))
                .Returns(new List<UserResponse>
                {
            new UserResponse
            {
                UserId = "user-1",
                FavouriteType = new List<TagListResponse>()
            }
                });

            var query = new GetAllUser
            {
                Page = 0,
                Limit = 10,
                SearchTerm = "",
                SortBy = ""
            };

            // Act
            var result = await _handler.Handle(query, default);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("Retrieved");

            var dataType = result.Data.GetType();
            var usersProp = dataType.GetProperty("Users");
            usersProp.ShouldNotBeNull();

            var userList = usersProp.GetValue(result.Data) as List<UserResponse>;
            userList.ShouldNotBeNull();
            userList!.Count.ShouldBe(1);
            userList[0].FavouriteType.ShouldContain(t => t.TagId == "tag-1" && t.Name == "Fantasy");
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Return_Fail_When_No_Users_Found()
        {
            // Arrange
            _userRepoMock.Setup(r => r.GetAllUserAsync(It.IsAny<FindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync((new List<UserEntity>(), 0));

            var query = new GetAllUser
            {
                Page = 0,
                Limit = 10,
                SearchTerm = "nothing"
            };

            // Act
            var result = await _handler.Handle(query, default);

            // Assert
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("No users found.");
        }

        [Fact]
        public async Task Handle_Should_Fallback_To_Fuzzy_If_Exact_Not_Found()
        {
            // Arrange
            var users = new List<UserEntity>
            {
                new UserEntity
                {
                    id = "user-1",
                    role = Role.User,
                    favourite_type = new List<string> { "tag-2" }
                }
            };

            _userRepoMock.SetupSequence(r => r.GetAllUserAsync(It.IsAny<FindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync((new List<UserEntity>(), 0))
                .ReturnsAsync((users, users.Count));
            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity>
                {
            new TagEntity { id = "tag-2", name = "Action" }
                });

            _mapperMock.Setup(m => m.Map<List<UserResponse>>(It.IsAny<List<UserEntity>>()))
                .Returns(new List<UserResponse>
                {
            new UserResponse { UserId = "user-1", FavouriteType = new List<TagListResponse>() }
                });

            var query = new GetAllUser
            {
                Page = 0,
                Limit = 10,
                SearchTerm = "sword"
            };

            // Act
            var result = await _handler.Handle(query, default);

            // Assert
            result.Success.ShouldBeTrue();

            var usersProperty = result.Data?.GetType().GetProperty("Users");
            usersProperty.ShouldNotBeNull();

            var usersList = usersProperty?.GetValue(result.Data) as List<UserResponse>;
            usersList.ShouldNotBeNull();
            usersList!.Count.ShouldBe(1);
            usersList[0].UserId.ShouldBe("user-1");
        }
    }
}
