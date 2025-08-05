
using Application.Features.Forum.Commands;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Commands
{
    public class DeletePostCommentCommandHandlerTests
    {
        private readonly Mock<IForumCommentRepository> _commentRepoMock;
        private readonly Mock<IForumPostRepository> _postRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly DeletePostCommentCommandHandler _handler;

        public DeletePostCommentCommandHandlerTests()
        {
            _commentRepoMock = new Mock<IForumCommentRepository>();
            _postRepoMock = new Mock<IForumPostRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _handler = new DeletePostCommentCommandHandler(
                _commentRepoMock.Object,
                _postRepoMock.Object,
                _currentUserMock.Object
            );
        }

        // ----------- HAPPY PATH -----------

        [Fact]
        public async Task Handle_Should_Delete_Comment_Without_Replies_Successfully()
        {
            var command = new DeletePostCommentCommand { Id = "cmt123" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("cmt123"))
                            .ReturnsAsync(new ForumCommentEntity { id = "cmt123", user_id = "user123", post_id = "post123" });

            _currentUserMock.Setup(u => u.UserId).Returns("user123");
            _currentUserMock.Setup(u => u.IsAdmin()).Returns(false);

            _commentRepoMock.Setup(c => c.GetReplyIdsByCommentIdAsync("cmt123"))
                            .ReturnsAsync(new List<string>());

            _commentRepoMock.Setup(c => c.DeleteAsync("cmt123"))
                            .ReturnsAsync(true);

            _postRepoMock.Setup(p => p.DecrementCommentsAsync("post123", 1))
                            .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment deleted successfully.");
        }

        [Fact]
        public async Task Handle_Should_Delete_Comment_With_Replies_Successfully()
        {
            var command = new DeletePostCommentCommand { Id = "cmt123" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("cmt123"))
                            .ReturnsAsync(new ForumCommentEntity { id = "cmt123", user_id = "user123", post_id = "post123" });

            _currentUserMock.Setup(u => u.UserId).Returns("user123");
            _currentUserMock.Setup(u => u.IsAdmin()).Returns(false);

            _commentRepoMock.Setup(c => c.GetReplyIdsByCommentIdAsync("cmt123"))
                            .ReturnsAsync(new List<string> { "child1", "child2" });

            _commentRepoMock.Setup(c => c.DeleteManyAsync(It.IsAny<List<string>>()))
                            .ReturnsAsync(true);

            _commentRepoMock.Setup(c => c.DeleteAsync("cmt123"))
                            .ReturnsAsync(true);

            _postRepoMock.Setup(p => p.DecrementCommentsAsync("post123", 3))
                            .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment deleted successfully.");
        }

        [Fact]
        public async Task Handle_Should_Delete_Comment_As_Admin_Successfully()
        {
            var command = new DeletePostCommentCommand { Id = "cmt123" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("cmt123"))
                            .ReturnsAsync(new ForumCommentEntity { id = "cmt123", user_id = "otherUser", post_id = "post123" });

            _currentUserMock.Setup(u => u.IsAdmin()).Returns(true);

            _commentRepoMock.Setup(c => c.GetReplyIdsByCommentIdAsync("cmt123"))
                            .ReturnsAsync(new List<string>());

            _commentRepoMock.Setup(c => c.DeleteAsync("cmt123"))
                            .ReturnsAsync(true);

            _postRepoMock.Setup(p => p.DecrementCommentsAsync("post123", 1))
                            .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment deleted successfully.");
        }

        // ----------- FAILURE CASES -----------

        [Fact]
        public async Task Handle_Should_Fail_When_Comment_Not_Found()
        {
            var command = new DeletePostCommentCommand { Id = "invalid" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("invalid"))
                            .ReturnsAsync((ForumCommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Owner_And_Not_Admin()
        {
            var command = new DeletePostCommentCommand { Id = "cmt123" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("cmt123"))
                            .ReturnsAsync(new ForumCommentEntity { id = "cmt123", user_id = "owner" });

            _currentUserMock.Setup(u => u.UserId).Returns("otherUser");
            _currentUserMock.Setup(u => u.IsAdmin()).Returns(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User is not allowed to delete this comment.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_DeleteAsync_Fails()
        {
            var command = new DeletePostCommentCommand { Id = "cmt123" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("cmt123"))
                            .ReturnsAsync(new ForumCommentEntity { id = "cmt123", user_id = "user123", post_id = "post123" });

            _currentUserMock.Setup(u => u.UserId).Returns("user123");
            _currentUserMock.Setup(u => u.IsAdmin()).Returns(false);

            _commentRepoMock.Setup(c => c.GetReplyIdsByCommentIdAsync("cmt123"))
                            .ReturnsAsync(new List<string>());

            _commentRepoMock.Setup(c => c.DeleteAsync("cmt123"))
                            .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to delete the comment.");
        }
    }
}