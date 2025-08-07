using Application.Features.Forum.Commands;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Commands
{
    public class UpdatePostCommentCommandHandlerTests
    {
        private readonly Mock<IForumCommentRepository> _commentRepoMock;
        private readonly UpdatePostCommentCommandHandler _handler;

        public UpdatePostCommentCommandHandlerTests()
        {
            _commentRepoMock = new Mock<IForumCommentRepository>();
            _handler = new UpdatePostCommentCommandHandler(_commentRepoMock.Object);
        }

        // ----------------- HAPPY PATH -----------------

        [Fact]
        public async Task Handle_Should_Update_Comment_Successfully()
        {
            // Arrange
            var command = new UpdatePostCommentCommand
            {
                Id = "comment123",
                UserId = "user123",
                Content = "Updated content"
            };

            _commentRepoMock.Setup(c => c.GetByIdAsync("comment123"))
                .ReturnsAsync(new ForumCommentEntity
                {
                    id = "comment123",
                    user_id = "user123",
                    content = "Old content"
                });

            _commentRepoMock.Setup(c => c.UpdateAsync("comment123", It.IsAny<ForumCommentEntity>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment updated successfully.");
        }

        // ----------------- FAILURE CASES -----------------

        [Fact]
        public async Task Handle_Should_Fail_When_Content_Is_Empty()
        {
            var command = new UpdatePostCommentCommand
            {
                Id = "comment123",
                UserId = "user123",
                Content = "" // Empty content
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Content cannot be empty.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Comment_Not_Found()
        {
            var command = new UpdatePostCommentCommand
            {
                Id = "invalid_comment",
                UserId = "user123",
                Content = "Updated content"
            };

            _commentRepoMock.Setup(c => c.GetByIdAsync("invalid_comment"))
                .ReturnsAsync((ForumCommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Comment_Owner()
        {
            var command = new UpdatePostCommentCommand
            {
                Id = "comment123",
                UserId = "user456",
                Content = "Updated content"
            };

            _commentRepoMock.Setup(c => c.GetByIdAsync("comment123"))
                .ReturnsAsync(new ForumCommentEntity
                {
                    id = "comment123",
                    user_id = "user123", 
                    content = "Old content"
                });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("You are not allowed to edit this comment.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_UpdateAsync_Returns_False()
        {
            var command = new UpdatePostCommentCommand
            {
                Id = "comment123",
                UserId = "user123",
                Content = "Updated content"
            };

            _commentRepoMock.Setup(c => c.GetByIdAsync("comment123"))
                .ReturnsAsync(new ForumCommentEntity
                {
                    id = "comment123",
                    user_id = "user123",
                    content = "Old content"
                });

            _commentRepoMock.Setup(c => c.UpdateAsync("comment123", It.IsAny<ForumCommentEntity>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to update the comment.");
        }
    }
}
