using Application.Features.Comment.Commands;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Comment.Commands
{
    public class UnlikeCommentHandlerTests
    {
        private readonly Mock<ICommentLikeRepository> _likeRepoMock = new();
        private readonly Mock<ICommentRepository> _commentRepoMock = new();
        private readonly UnlikeCommentCommandHandler _handler;

        public UnlikeCommentHandlerTests()
        {
            _handler = new UnlikeCommentCommandHandler(
                _likeRepoMock.Object,
                _commentRepoMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Missing_Fields()
        {
            var command = new UnlikeCommentCommand
            {
                CommentId = null,
                UserId = null
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Missing required fields: CommentId or UserId.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Comment_NotFound()
        {
            var command = new UnlikeCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync((CommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment does not exist.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Has_Not_Liked()
        {
            var command = new UnlikeCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(new CommentEntity());

            _likeRepoMock.Setup(r => r.HasUserLikedCommentAsync("cmt-1", "user1"))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User has not liked this comment.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Unlike_Failed()
        {
            var command = new UnlikeCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(new CommentEntity());

            _likeRepoMock.Setup(r => r.HasUserLikedCommentAsync("cmt-1", "user1"))
                .ReturnsAsync(true);

            _likeRepoMock.Setup(r => r.UnlikeCommentAsync("cmt-1", "user1"))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to unlike the comment.");
        }

        [Fact]
        public async Task Handle_Should_Succeed_When_Valid()
        {
            var command = new UnlikeCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(new CommentEntity());

            _likeRepoMock.Setup(r => r.HasUserLikedCommentAsync("cmt-1", "user1"))
                .ReturnsAsync(true);

            _likeRepoMock.Setup(r => r.UnlikeCommentAsync("cmt-1", "user1"))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Unlike successfully.");
        }
    }
}
