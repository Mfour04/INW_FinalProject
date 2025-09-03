using Application.Features.Comment.Commands;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Comment.Commands
{
    public class LikeCommentHandlerTests
    {
        private readonly Mock<ICommentRepository> _commentRepoMock = new();
        private readonly Mock<ICommentLikeRepository> _likeRepoMock = new();
        private readonly LikeCommentCommandHandler _handler;

        public LikeCommentHandlerTests()
        {
            _handler = new LikeCommentCommandHandler(
                _likeRepoMock.Object,
                _commentRepoMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Missing_Fields()
        {
            var command = new LikeCommentCommand
            {
                CommentId = null,
                UserId = null,
                Type = (int)CommentType.Novel
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Missing required fields: CommentId or UserId.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Invalid_CommentType()
        {
            var command = new LikeCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1",
                Type = 999 // invalid
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Invalid or unsupported comment type.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Comment_NotFound()
        {
            var command = new LikeCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1",
                Type = (int)CommentType.Novel
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync((CommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment does not exist.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Already_Liked()
        {
            var command = new LikeCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1",
                Type = (int)CommentType.Novel
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(new CommentEntity());

            _likeRepoMock.Setup(r => r.HasUserLikedCommentAsync("cmt-1", "user1"))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User has already liked this comment.");
        }

        [Fact]
        public async Task Handle_Should_Succeed_When_Valid()
        {
            var command = new LikeCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1",
                Type = (int)CommentType.Chapter
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(new CommentEntity());

            _likeRepoMock.Setup(r => r.HasUserLikedCommentAsync("cmt-1", "user1"))
                .ReturnsAsync(false);

            _likeRepoMock.Setup(r => r.LikeCommentAsync(It.IsAny<CommentLikeEntity>()))
                .ReturnsAsync((CommentLikeEntity input) => input);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Like successfully.");
            result.Data.ShouldBeOfType<CommentLikeEntity>();
        }
    }
}
