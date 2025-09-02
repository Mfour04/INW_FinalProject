using Application.Features.Novel.Commands;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Novel.Commands
{
    public class DeleteNovelHandlerTests
    {
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<ICloudDinaryService> _cloudServiceMock;
        private readonly Mock<IOpenAIRepository> _openAIRepoMock;
        private readonly DeleteNovelHandler _handler;

        public DeleteNovelHandlerTests()
        {
            _novelRepoMock = new Mock<INovelRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _cloudServiceMock = new Mock<ICloudDinaryService>();
            _openAIRepoMock = new Mock<IOpenAIRepository>();

            _handler = new DeleteNovelHandler(
                _novelRepoMock.Object,
                _currentUserMock.Object,
                _cloudServiceMock.Object,
                _openAIRepoMock.Object
            );
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Not_Found()
        {
            var command = new DeleteNovelCommand { NovelId = "n1" };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync((NovelEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Novel not found");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Author()
        {
            var command = new DeleteNovelCommand { NovelId = "n1" };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", author_id = "author1" });

            _currentUserMock.Setup(c => c.UserId).Returns("differentUser");

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Unauthorized: You are not the author of this novel");
        }

        // ---------- SUCCESS CASE ----------

        [Fact]
        public async Task Handle_Should_Delete_Novel_Successfully()
        {
            var command = new DeleteNovelCommand { NovelId = "n1" };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity
                {
                    id = "n1",
                    author_id = "a1",
                    novel_image = "img",
                    novel_banner = "banner"
                });

            _currentUserMock.Setup(c => c.UserId).Returns("a1");

            _novelRepoMock.Setup(n => n.DeleteNovelAsync("n1"))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Novel Deleted Succuessfully");
            _cloudServiceMock.Verify(c => c.DeleteImageAsync("img"), Times.Once);
            _cloudServiceMock.Verify(c => c.DeleteImageAsync("banner"), Times.Once);
            _openAIRepoMock.Verify(o => o.DeleteNovelEmbeddingAsync("n1"), Times.Once);
        }
    }
}
