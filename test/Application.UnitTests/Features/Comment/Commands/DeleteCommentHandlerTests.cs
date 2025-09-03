using Application.Features.Comment.Commands;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Comment.Commands
{
    public class DeleteCommentHandlerTests
    {
        private readonly Mock<ICommentRepository> _commentRepoMock = new();
        private readonly Mock<IChapterRepository> _chapterRepoMock = new();
        private readonly Mock<INovelRepository> _novelRepoMock = new();
        private readonly Mock<ICurrentUserService> _currentUserMock = new();

        private readonly DeleteCommentCommandHandler _handler;

        public DeleteCommentHandlerTests()
        {
            _handler = new DeleteCommentCommandHandler(
                _commentRepoMock.Object,
                _novelRepoMock.Object,
                _chapterRepoMock.Object,
                _currentUserMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Comment_NotFound()
        {
            var command = new DeleteCommentCommand { CommentId = "cmt-1" };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync((CommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_IsNotAuthor_And_NotAdmin()
        {
            var comment = new CommentEntity
            {
                id = "cmt-1",
                user_id = "user1"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(comment);

            _currentUserMock.Setup(c => c.UserId).Returns("user2");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(false);

            var result = await _handler.Handle(new DeleteCommentCommand { CommentId = "cmt-1" }, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("You are not authorized to delete this comment.");
        }

        [Fact]
        public async Task Handle_Should_Succeed_For_Admin_And_CommentWithoutReplies()
        {
            var comment = new CommentEntity
            {
                id = "cmt-1",
                user_id = "user1",
                novel_id = "novel1",
                parent_comment_id = null
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(comment);

            _currentUserMock.Setup(c => c.IsAdmin()).Returns(true);

            _commentRepoMock.Setup(r => r.GetReplyIdsByParentIdAsync("cmt-1"))
                .ReturnsAsync(new List<string>());

            _commentRepoMock.Setup(r => r.DeleteAsync("cmt-1")).ReturnsAsync(true);

            _novelRepoMock.Setup(r => r.DecrementCommentsAsync("novel1", 1))
                .ReturnsAsync(true);

            var result = await _handler.Handle(new DeleteCommentCommand { CommentId = "cmt-1" }, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment Deleted Successfully");

            _commentRepoMock.Verify(r => r.DeleteAsync("cmt-1"), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Succeed_And_Delete_Replies()
        {
            var comment = new CommentEntity
            {
                id = "cmt-1",
                user_id = "user1",
                chapter_id = "chapter-1",
                parent_comment_id = null
            };

            var replyIds = new List<string> { "r1", "r2" };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(comment);

            _currentUserMock.Setup(c => c.UserId).Returns("user1");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(false);

            _commentRepoMock.Setup(r => r.GetReplyIdsByParentIdAsync("cmt-1"))
                .ReturnsAsync(replyIds);

            _commentRepoMock.Setup(r => r.DeleteManyAsync(replyIds)).Returns(Task.CompletedTask);
            _commentRepoMock.Setup(r => r.DeleteAsync("cmt-1")).ReturnsAsync(true);
            _commentRepoMock.Setup(r => r.DeleteRepliesByParentIdAsync("cmt-1")).ReturnsAsync(true);

            _chapterRepoMock.Setup(r => r.DecrementCommentsAsync("chapter-1", 3))
                .ReturnsAsync(true);

            var result = await _handler.Handle(new DeleteCommentCommand { CommentId = "cmt-1" }, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment Deleted Successfully");

            _commentRepoMock.Verify(r => r.DeleteManyAsync(replyIds), Times.Once);
            _commentRepoMock.Verify(r => r.DeleteRepliesByParentIdAsync("cmt-1"), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Delete_Fails()
        {
            var comment = new CommentEntity
            {
                id = "cmt-1",
                user_id = "user1",
                novel_id = "novel1"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("cmt-1"))
                .ReturnsAsync(comment);

            _currentUserMock.Setup(c => c.UserId).Returns("user1");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(false);

            _commentRepoMock.Setup(r => r.GetReplyIdsByParentIdAsync("cmt-1"))
                .ReturnsAsync(new List<string>());

            _commentRepoMock.Setup(r => r.DeleteAsync("cmt-1"))
                .ReturnsAsync(false);

            var result = await _handler.Handle(new DeleteCommentCommand { CommentId = "cmt-1" }, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to delete comment.");
        }
    }
}
