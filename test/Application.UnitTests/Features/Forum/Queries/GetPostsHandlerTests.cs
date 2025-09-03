using Application.Features.Forum.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Queries
{
    public class GetPostsHandlerTests
    {
        private readonly Mock<IForumPostRepository> _postRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPostsHandler _handler;

        public GetPostsHandlerTests()
        {
            _postRepoMock = new Mock<IForumPostRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetPostsHandler(
                _postRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object
            );
        }

        // ----------------- FAILURE CASE -----------------

        [Fact]
        public async Task Handle_Should_Fail_When_No_Posts()
        {
            // Arrange
            var query = new GetPosts
            {
                Page = 0,
                Limit = 10
            };

            _postRepoMock.Setup(p =>
                p.GetAllAsync(
                    It.IsAny<FindCreterias>(),
                    It.IsAny<List<SortCreterias>>()
                )
            ).ReturnsAsync(new List<ForumPostEntity>()); 

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("No forum posts found.");
        }

        // ----------------- HAPPY PATH -----------------

        [Fact]
        public async Task Handle_Should_Return_Posts_Successfully()
        {
            // Arrange
            var query = new GetPosts
            {
                Page = 0,
                Limit = 10
            };

            var posts = new List<ForumPostEntity>
            {
                new ForumPostEntity { id = "p1", user_id = "u1" },
                new ForumPostEntity { id = "p2", user_id = "u2" }
            };

            _postRepoMock.Setup(p =>
                p.GetAllAsync(
                    It.IsAny<FindCreterias>(),
                    It.IsAny<List<SortCreterias>>()
                )
            ).ReturnsAsync(posts);

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity>
                {
                    new UserEntity { id = "u1", username = "user1", avata_url = "avatar1" },
                    new UserEntity { id = "u2", username = "user2", avata_url = "avatar2" }
                });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostResponse>(It.IsAny<ForumPostEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostResponse());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Retrieved forum posts successfully.");
            result.Data.ShouldNotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_Posts_With_Missing_User_Info()
        {
            // Arrange
            var query = new GetPosts
            {
                Page = 0,
                Limit = 10
            };

            var posts = new List<ForumPostEntity>
            {
                new ForumPostEntity { id = "p1", user_id = "u1" },
                new ForumPostEntity { id = "p2", user_id = "u2" }
            };

            _postRepoMock.Setup(p =>
                p.GetAllAsync(
                    It.IsAny<FindCreterias>(),
                    It.IsAny<List<SortCreterias>>()
                )
            ).ReturnsAsync(posts);

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity>
                {
                    new UserEntity { id = "u1", username = "user1", avata_url = "avatar1" }
                });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostResponse>(It.IsAny<ForumPostEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostResponse());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Retrieved forum posts successfully.");
            result.Data.ShouldNotBeNull();
        }
    }
}
