using Application.Features.Forum.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Queries
{
    public class GetPostCommentsHandlerTests
    {
        private readonly Mock<IForumCommentRepository> _commentRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPostCommentsHandler _handler;

        public GetPostCommentsHandlerTests()
        {
            _commentRepoMock = new Mock<IForumCommentRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetPostCommentsHandler(
                _commentRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object
            );
        }

        // ----------------- FAILURE CASE -----------------

        [Fact]
        public async Task Handle_Should_Fail_When_PostId_Is_Missing()
        {
            // Arrange
            var query = new GetPostComments
            {
                PostId = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("PostId is required.");
        }

        // ----------------- HAPPY PATH -----------------

        [Fact]
        public async Task Handle_Should_Return_Empty_When_No_Comments()
        {
            var query = new GetPostComments
            {
                PostId = "post123",
                Page = 0,
                Limit = 10
            };

            _commentRepoMock.Setup(c =>
                c.GetRootCommentsByPostIdAsync(
                    "post123",
                    It.IsAny<FindCreterias>(),
                    It.IsAny<List<SortCreterias>>() 
                )
            ).ReturnsAsync(new List<ForumCommentEntity>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("No Post's comments found.");
        }

        [Fact]
        public async Task Handle_Should_Return_Comments_Successfully()
        {
            // Arrange
            var query = new GetPostComments
            {
                PostId = "post123",
                Page = 0,
                Limit = 10
            };

            var comments = new List<ForumCommentEntity>
            {
                new ForumCommentEntity { id = "c1", user_id = "u1", post_id = "post123" },
                new ForumCommentEntity { id = "c2", user_id = "u2", post_id = "post123" }
            };

            _commentRepoMock.Setup(c =>
                c.GetRootCommentsByPostIdAsync(
                    "post123",
                    It.IsAny<FindCreterias>(),
                    It.IsAny<List<SortCreterias>>()
                )
            ).ReturnsAsync(comments);

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity>
                {
                    new UserEntity { id = "u1", username = "user1", displayname = "User One", avata_url = "avatar1" },
                    new UserEntity { id = "u2", username = "user2", displayname = "User Two", avata_url = "avatar2" }
                });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostRootCommentResponse>(It.IsAny<ForumCommentEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostRootCommentResponse());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Retrieved comments successfully.");
            result.Data.ShouldNotBeNull();
        }
    }
}
