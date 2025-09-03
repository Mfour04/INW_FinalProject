using Application.Features.Novel.Commands;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Novel.Commands
{
    public class HideNovelCommandHandlerTests
    {
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IChapterRepository> _chapterRepoMock;
        private readonly HideNovelCommandHandler _handler;

        public HideNovelCommandHandlerTests()
        {
            _novelRepoMock = new Mock<INovelRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _chapterRepoMock = new Mock<IChapterRepository>();
            _handler = new HideNovelCommandHandler(
                _novelRepoMock.Object,
                _currentUserMock.Object,
                _chapterRepoMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_UserId_Is_Empty()
        {
            _currentUserMock.Setup(c => c.UserId).Returns(string.Empty);

            var command = new UpdateHideNovelCommand { NovelId = "n1", IsPublic = false };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Unauthorized");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Not_Found()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("u1");
            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync((NovelEntity?)null);

            var command = new UpdateHideNovelCommand { NovelId = "n1", IsPublic = false };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Novel not found");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Author()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("u2");
            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", author_id = "u1" });

            var command = new UpdateHideNovelCommand { NovelId = "n1", IsPublic = false };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Forbidden: You are not the author of this novel.");
        }

        [Fact]
        public async Task Handle_Should_Update_Hide_Status_Successfully()
        {
            _currentUserMock.Setup(c => c.UserId).Returns("u1");
            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", author_id = "u1" });

            var command = new UpdateHideNovelCommand { NovelId = "n1", IsPublic = false };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("hidden successfully");
            _novelRepoMock.Verify(n => n.UpdateHideNovelAsync("n1", false), Times.Once);
            _chapterRepoMock.Verify(c => c.UpdateHideAllChaptersByNovelIdAsync("n1", false), Times.Once);
        }
    }
}
