using Application.Features.Forum.Commands;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Commands
{
    public class UnlikePostCommentCommandHandlerTests
    {
        private readonly Mock<ICommentLikeRepository> _commentLikeRepoMock;
        private readonly Mock<IForumCommentRepository> _commentRepoMock;
        private readonly UnlikePostCommentCommandHandler _handler;

        public UnlikePostCommentCommandHandlerTests()
        {
            _commentLikeRepoMock = new Mock<ICommentLikeRepository>();
            _commentRepoMock = new Mock<IForumCommentRepository>();
            _handler = new UnlikePostCommentCommandHandler(_commentLikeRepoMock.Object, _commentRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Missing_Fields()
        {
            var command = new UnlikePostCommentCommand { CommentId = "", UserId = "" };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Missing required fields: CommentId or UserId.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Comment_Not_Exist()
        {
            var command = new UnlikePostCommentCommand { CommentId = "c1", UserId = "u1" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("c1"))
                .ReturnsAsync((ForumCommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Comment does not exist.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Liked_Comment()
        {
            var command = new UnlikePostCommentCommand { CommentId = "c1", UserId = "u1" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("c1"))
                .ReturnsAsync(new ForumCommentEntity { id = "c1" });

            _commentLikeRepoMock.Setup(l => l.HasUserLikedCommentAsync("c1", "u1"))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User has not liked this comment.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Unlike_Fails()
        {
            var command = new UnlikePostCommentCommand { CommentId = "c1", UserId = "u1" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("c1"))
                .ReturnsAsync(new ForumCommentEntity { id = "c1" });

            _commentLikeRepoMock.Setup(l => l.HasUserLikedCommentAsync("c1", "u1"))
                .ReturnsAsync(true);

            _commentLikeRepoMock.Setup(l => l.UnlikeCommentAsync("c1", "u1"))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to unlike the comment.");
        }

        [Fact]
        public async Task Handle_Should_Unlike_Comment_Successfully()
        {
            var command = new UnlikePostCommentCommand { CommentId = "c1", UserId = "u1" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("c1"))
                .ReturnsAsync(new ForumCommentEntity { id = "c1" });

            _commentLikeRepoMock.Setup(l => l.HasUserLikedCommentAsync("c1", "u1"))
                .ReturnsAsync(true);

            _commentLikeRepoMock.Setup(l => l.UnlikeCommentAsync("c1", "u1"))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Unlike successfully.");
        }
    }
}
