using Application.Features.Forum.Commands;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Commands
{
    public class DeletePostCommandHandlerTests
    {
        private readonly Mock<IForumPostRepository> _postRepoMock;
        private readonly Mock<ICloudDinaryService> _cloudServiceMock;
        private readonly DeletePostCommandHandler _handler;

        public DeletePostCommandHandlerTests()
        {
            _postRepoMock = new Mock<IForumPostRepository>();
            _cloudServiceMock = new Mock<ICloudDinaryService>();

            _handler = new DeletePostCommandHandler(_postRepoMock.Object, _cloudServiceMock.Object);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Post_Not_Found()
        {
            var command = new DeletePostCommand { Id = "p1", UserId = "u1" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync((ForumPostEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Post not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Owner()
        {
            var command = new DeletePostCommand { Id = "p1", UserId = "u2" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity { id = "p1", user_id = "u1" });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User is not allowed to delete this post.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_DeleteAsync_Returns_False()
        {
            var command = new DeletePostCommand { Id = "p1", UserId = "u1" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity { id = "p1", user_id = "u1" });

            _postRepoMock.Setup(p => p.DeleteAsync("p1"))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to delete the post.");
        }

        [Fact]
        public async Task Handle_Should_Delete_Post_And_Images_Successfully()
        {
            var command = new DeletePostCommand { Id = "p1", UserId = "u1" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity
                {
                    id = "p1",
                    user_id = "u1",
                    img_urls = new List<string> { "img1", "img2" }
                });

            _postRepoMock.Setup(p => p.DeleteAsync("p1"))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Post deleted successfully.");
            _cloudServiceMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
