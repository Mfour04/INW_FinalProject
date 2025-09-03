using Application.Features.Forum.Commands;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Commands
{
    public class UpdatePostCommandHandlerTests
    {
        private readonly Mock<IForumPostRepository> _postRepoMock;
        private readonly UpdatePostCommandHandler _handler;

        public UpdatePostCommandHandlerTests()
        {
            _postRepoMock = new Mock<IForumPostRepository>();
            _handler = new UpdatePostCommandHandler(_postRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Content_Is_Empty()
        {
            var command = new UpdatePostCommand { Id = "p1", UserId = "u1", Content = "" };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Content cannot be empty.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Post_Not_Found()
        {
            var command = new UpdatePostCommand { Id = "p1", UserId = "u1", Content = "Updated content" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync((ForumPostEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Post not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Owner()
        {
            var command = new UpdatePostCommand { Id = "p1", UserId = "u2", Content = "Updated content" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity { id = "p1", user_id = "u1" });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("You are not allowed to edit this post.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Update_Fails()
        {
            var command = new UpdatePostCommand { Id = "p1", UserId = "u1", Content = "Updated content" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity { id = "p1", user_id = "u1" });

            _postRepoMock.Setup(p => p.UpdateAsync("p1", It.IsAny<ForumPostEntity>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to update the post.");
        }

        [Fact]
        public async Task Handle_Should_Update_Post_Successfully()
        {
            var command = new UpdatePostCommand { Id = "p1", UserId = "u1", Content = "Updated content" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity { id = "p1", user_id = "u1" });

            _postRepoMock.Setup(p => p.UpdateAsync("p1", It.IsAny<ForumPostEntity>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Post updated successfully.");
        }
    }
}
