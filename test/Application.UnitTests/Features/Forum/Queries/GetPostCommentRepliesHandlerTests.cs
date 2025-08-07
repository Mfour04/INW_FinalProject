using Application.Features.Forum.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Queries
{
    public class GetPostCommentRepliesHandlerTests
    {
        private readonly Mock<IForumCommentRepository> _commentRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPostCommentRepliesHandler _handler;

        public GetPostCommentRepliesHandlerTests()
        {
            _commentRepoMock = new Mock<IForumCommentRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetPostCommentRepliesHandler(
                _commentRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object
            );
        }

        // ----------------- FAILURE CASE -----------------

        [Fact]
        public async Task Handle_Should_Fail_When_ParentId_Is_Missing()
        {
            // Arrange
            var query = new GetPostCommentReplies
            {
                ParentId = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("ParentId is required.");
        }

        // ----------------- HAPPY PATH -----------------

        [Fact]
        public async Task Handle_Should_Return_Empty_When_No_Replies()
        {
            var query = new GetPostCommentReplies
            {
                ParentId = "parent123",
                Page = 0,
                Limit = 10
            };

            _commentRepoMock.Setup(c =>
                c.GetRepliesByCommentIdAsync(
                    "parent123",
                    It.IsAny<FindCreterias>(),
                    It.IsAny<List<SortCreterias>>()
                )
            ).ReturnsAsync(new List<ForumCommentEntity>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("No Comment's reply found.");
        }

        [Fact]
        public async Task Handle_Should_Return_Replies_Successfully()
        {
            // Arrange
            var query = new GetPostCommentReplies
            {
                ParentId = "parent123",
                Page = 0,
                Limit = 10
            };

            var replies = new List<ForumCommentEntity>
            {
                new ForumCommentEntity { id = "r1", user_id = "u1", post_id = "post123", parent_comment_id = "parent123" },
                new ForumCommentEntity { id = "r2", user_id = "u2", post_id = "post123", parent_comment_id = "parent123" }
            };

            _commentRepoMock.Setup(c =>
                c.GetRepliesByCommentIdAsync(
                    "parent123",
                    It.IsAny<FindCreterias>(),
                    It.IsAny<List<SortCreterias>>()
                )
            ).ReturnsAsync(replies);

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity>
                {
                    new UserEntity { id = "u1", username = "user1", displayname = "User One", avata_url = "avatar1" },
                    new UserEntity { id = "u2", username = "user2", displayname = "User Two", avata_url = "avatar2" }
                });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostReplyCommentResponse>(It.IsAny<ForumCommentEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostReplyCommentResponse());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Retrieved comment's replies successfully.");
            result.Data.ShouldNotBeNull();
        }
    }
}
