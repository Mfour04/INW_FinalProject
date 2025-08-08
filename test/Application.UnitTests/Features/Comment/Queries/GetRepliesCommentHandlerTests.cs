using Application.Features.Comment.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using Shouldly;
using Xunit;

namespace Application.UnitTests.Features.Comment.Queries
{
    public class GetRepliesCommentHandlerTests
    {
        private readonly Mock<ICommentRepository> _commentRepoMock = new();
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly GetRepliesCommentHandler _handler;

        public GetRepliesCommentHandlerTests()
        {
            _handler = new GetRepliesCommentHandler(
                _commentRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object
            );
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Return_Fail_When_Comment_Not_Found()
        {
            var request = new GetRepliesComment { CommentId = "c1" };

            _commentRepoMock.Setup(r => r.GetByIdAsync("c1"))
                .ReturnsAsync((CommentEntity?)null);

            var result = await _handler.Handle(request, default);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment not found.");
        }

        [Fact]
        public async Task Handle_Should_Return_Fail_When_No_Replies_Found()
        {
            var request = new GetRepliesComment { CommentId = "c1" };

            _commentRepoMock.Setup(r => r.GetByIdAsync("c1"))
                .ReturnsAsync(new CommentEntity { id = "c1" });

            _commentRepoMock.Setup(r =>
                r.GetRepliesByCommentIdAsync("c1", It.IsAny<FindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync(new List<CommentEntity>());

            var result = await _handler.Handle(request, default);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("No Comment's reply found.");
        }

        // ---------- HAPPY PATH ----------

        [Fact]
        public async Task Handle_Should_Return_Replies_With_Authors_When_Found()
        {
            var request = new GetRepliesComment
            {
                CommentId = "c1",
                Page = 0,
                Limit = 10,
                SortBy = "created_at:desc"
            };

            var replies = new List<CommentEntity>
            {
                new CommentEntity
                {
                    id = "r1",
                    parent_comment_id = "c1",
                    user_id = "u1",
                    content = "reply 1"
                },
                new CommentEntity
                {
                    id = "r2",
                    parent_comment_id = "c1",
                    user_id = "u2",
                    content = "reply 2"
                }
            };

            var users = new List<UserEntity>
            {
                new UserEntity { id = "u1", username = "user1", displayname = "User One", avata_url = "url1" },
                new UserEntity { id = "u2", username = "user2", displayname = "User Two", avata_url = "url2" }
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("c1"))
                .ReturnsAsync(new CommentEntity { id = "c1" });

            _commentRepoMock.Setup(r =>
                r.GetRepliesByCommentIdAsync("c1", It.IsAny<FindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync(replies);

            _userRepoMock.Setup(r => r.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(users);

            _mapperMock.Setup(m => m.Map<CommentReplyResponse>(It.Is<CommentEntity>(c => c.id == "r1")))
                .Returns(new CommentReplyResponse { Id = "r1", Content = "reply 1" });

            _mapperMock.Setup(m => m.Map<CommentReplyResponse>(It.Is<CommentEntity>(c => c.id == "r2")))
                .Returns(new CommentReplyResponse { Id = "r2", Content = "reply 2" });

            var result = await _handler.Handle(request, default);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Retrieved reply comments successfully.");

            var data = result.Data as List<CommentReplyResponse>;
            data.ShouldNotBeNull();
            data.Count.ShouldBe(2);
            data[0].Author.ShouldNotBeNull();
            data[0].Author.DisplayName.ShouldBe("User One");
            data[1].Author.DisplayName.ShouldBe("User Two");
        }
    }
}
