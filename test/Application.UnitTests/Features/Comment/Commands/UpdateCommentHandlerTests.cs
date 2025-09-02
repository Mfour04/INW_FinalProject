using Application.Features.Comment.Commands;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Comment.Commands
{
    public class UpdateCommentHandlerTests
    {
        private readonly Mock<ICommentRepository> _commentRepoMock = new();
        private readonly UpdateCommentHandler _handler;

        public UpdateCommentHandlerTests()
        {
            _handler = new UpdateCommentHandler(_commentRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Comment_NotFound()
        {
            var command = new UpdateCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1",
                Content = "New content"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync((CommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Is_Not_Author()
        {
            var existingComment = new CommentEntity
            {
                id = "cmt-1",
                user_id = "other-user",
                content = "Old content"
            };

            var command = new UpdateCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1", 
                Content = "New content"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(existingComment);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("You are not authorized to update this comment.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Update_Fails()
        {
            var comment = new CommentEntity
            {
                id = "cmt-1",
                user_id = "user1"
            };

            var command = new UpdateCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1",
                Content = "Updated comment"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(comment);

            _commentRepoMock.Setup(r => r.UpdateAsync("cmt-1", It.IsAny<CommentEntity>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to update the badge.");
        }

        [Fact]
        public async Task Handle_Should_Succeed_When_Valid()
        {
            var comment = new CommentEntity
            {
                id = "cmt-1",
                user_id = "user1"
            };

            var command = new UpdateCommentCommand
            {
                CommentId = "cmt-1",
                UserId = "user1",
                Content = "Updated content"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(comment);

            _commentRepoMock.Setup(r => r.UpdateAsync("cmt-1", It.IsAny<CommentEntity>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment Updated Successfully.");
        }
    }
}
