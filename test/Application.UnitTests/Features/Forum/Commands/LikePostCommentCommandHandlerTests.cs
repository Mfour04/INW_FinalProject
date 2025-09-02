using Application.Features.Forum.Commands;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Commands
{
    public class LikePostCommentCommandHandlerTests
    {
        private readonly Mock<ICommentLikeRepository> _commentLikeRepoMock;
        private readonly Mock<IForumCommentRepository> _commentRepoMock;
        private readonly LikePostCommentCommandHandler _handler;

        public LikePostCommentCommandHandlerTests()
        {
            _commentLikeRepoMock = new Mock<ICommentLikeRepository>();
            _commentRepoMock = new Mock<IForumCommentRepository>();
            _handler = new LikePostCommentCommandHandler(_commentLikeRepoMock.Object, _commentRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Missing_Fields()
        {
            var command = new LikePostCommentCommand { CommentId = "", UserId = "" };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Missing required fields: CommentId or UserId.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Comment_Not_Exist()
        {
            var command = new LikePostCommentCommand { CommentId = "c1", UserId = "u1" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("c1"))
                .ReturnsAsync((ForumCommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment does not exist.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Already_Liked()
        {
            var command = new LikePostCommentCommand { CommentId = "c1", UserId = "u1" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("c1"))
                .ReturnsAsync(new ForumCommentEntity { id = "c1" });

            _commentLikeRepoMock.Setup(l => l.HasUserLikedCommentAsync("c1", "u1"))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User has already liked this comment.");
        }

        [Fact]
        public async Task Handle_Should_Like_Comment_Successfully()
        {
            var command = new LikePostCommentCommand { CommentId = "c1", UserId = "u1" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("c1"))
                .ReturnsAsync(new ForumCommentEntity { id = "c1" });

            _commentLikeRepoMock.Setup(l => l.HasUserLikedCommentAsync("c1", "u1"))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Like successfully.");
            _commentLikeRepoMock.Verify(l => l.LikeCommentAsync(It.IsAny<CommentLikeEntity>()), Times.Once);
        }
    }
}
