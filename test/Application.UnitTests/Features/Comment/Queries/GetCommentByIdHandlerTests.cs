using Application.Features.Comment.Queries;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using Shouldly;
using Xunit;

namespace Application.UnitTests.Features.Comment.Queries
{
    public class GetCommentByIdHandlerTests
    {
        private readonly Mock<ICommentRepository> _commentRepoMock = new();
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly GetCommentByIdHandler _handler;

        public GetCommentByIdHandlerTests()
        {
            _handler = new GetCommentByIdHandler(
                _commentRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object
            );
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Return_Fail_When_Comment_Not_Found()
        {
            var query = new GetCommentById { CommentId = "c1" };

            _commentRepoMock.Setup(r => r.GetByIdAsync("c1"))
                .ReturnsAsync((CommentEntity?)null);

            var result = await _handler.Handle(query, default);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment not found.");
        }

        // ---------- HAPPY PATH ----------

        [Fact]
        public async Task Handle_Should_Return_Comment_With_User_And_ReplyCount()
        {
            var query = new GetCommentById { CommentId = "c1" };

            var comment = new CommentEntity
            {
                id = "c1",
                user_id = "u1",
                content = "Test comment"
            };

            var commentResponse = new CommentResponse
            {
                Id = "c1",
                Content = "Test comment"
            };

            var user = new UserEntity
            {
                id = "u1",
                username = "user1",
                displayname = "User One",
                avata_url = "http://image.com/avatar.jpg"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("c1"))
                .ReturnsAsync(comment);

            _mapperMock.Setup(m => m.Map<CommentResponse>(comment))
                .Returns(commentResponse);

            _commentRepoMock.Setup(r => r.CountRepliesPerCommentAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new Dictionary<string, int> { { "c1", 2 } });

            _userRepoMock.Setup(r => r.GetById("u1"))
                .ReturnsAsync(user);

            var result = await _handler.Handle(query, default);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment retrieved successfully.");

            var data = result.Data as CommentResponse;
            data.ShouldNotBeNull();
            data.Id.ShouldBe("c1");
            data.ReplyCount.ShouldBe(2);
            data.Author.ShouldNotBeNull();
            data.Author.Username.ShouldBe("user1");
            data.Author.DisplayName.ShouldBe("User One");
            data.Author.Avatar.ShouldBe("http://image.com/avatar.jpg");
        }
    }
}
