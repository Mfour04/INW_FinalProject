using Application.Features.Forum.Commands;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Commands
{
    public class LikePostCommandHandlerTests
    {
        private readonly Mock<IForumPostLikeRepository> _postLikeRepoMock;
        private readonly Mock<IForumPostRepository> _postRepoMock;
        private readonly LikePostCommandHandler _handler;

        public LikePostCommandHandlerTests()
        {
            _postLikeRepoMock = new Mock<IForumPostLikeRepository>();
            _postRepoMock = new Mock<IForumPostRepository>();
            _handler = new LikePostCommandHandler(_postLikeRepoMock.Object, _postRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Missing_Fields()
        {
            var command = new LikePostCommand { PostId = "", UserId = "" };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Missing required fields: PostId or UserId.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Post_Not_Exist()
        {
            var command = new LikePostCommand { PostId = "p1", UserId = "u1" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync((ForumPostEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Post does not exist.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Already_Liked()
        {
            var command = new LikePostCommand { PostId = "p1", UserId = "u1" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity { id = "p1" });

            _postLikeRepoMock.Setup(l => l.HasUserLikedPostAsync("p1", "u1"))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User has already liked this post.");
        }

        [Fact]
        public async Task Handle_Should_Like_Post_Successfully()
        {
            var command = new LikePostCommand { PostId = "p1", UserId = "u1" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity { id = "p1" });

            _postLikeRepoMock.Setup(l => l.HasUserLikedPostAsync("p1", "u1"))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Like successfully.");
            _postLikeRepoMock.Verify(l => l.LikePostAsync(It.IsAny<ForumPostLikeEntity>()), Times.Once);
            _postRepoMock.Verify(p => p.IncrementLikesAsync("p1"), Times.Once);
        }
    }
}
