using Application.Features.Novel.Commands;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Novel.Commands
{
    public class UpdateNovelHandleTests
    {
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICloudDinaryService> _cloudServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<ITagRepository> _tagRepoMock;
        private readonly UpdateNovelHandle _handler;

        public UpdateNovelHandleTests()
        {
            _novelRepoMock = new Mock<INovelRepository>();
            _mapperMock = new Mock<IMapper>();
            _cloudServiceMock = new Mock<ICloudDinaryService>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _tagRepoMock = new Mock<ITagRepository>();

            _handler = new UpdateNovelHandle(
                _novelRepoMock.Object,
                _mapperMock.Object,
                _cloudServiceMock.Object,
                _currentUserMock.Object,
                _tagRepoMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Not_Found()
        {
            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync((NovelEntity?)null);

            var command = new UpdateNovelCommand { NovelId = "n1", Title = "Title" };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Novel not found");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Author()
        {
            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", author_id = "author1" });

            _currentUserMock.Setup(c => c.UserId).Returns("otherUser");

            var command = new UpdateNovelCommand { NovelId = "n1", Title = "Title" };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Unauthorized: You are not the author of this novel");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Slug_Already_Exists()
        {
            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", author_id = "author1", slug = "old-slug" });

            _currentUserMock.Setup(c => c.UserId).Returns("author1");

            _novelRepoMock.Setup(n => n.IsSlugExistsAsync("new-slug", It.IsAny<string?>()))
                .ReturnsAsync(false);

            var command = new UpdateNovelCommand { NovelId = "n1", Slug = "new-slug" };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Slug already exists.");
        }

        [Fact]
        public async Task Handle_Should_Update_Novel_Successfully()
        {
            var novel = new NovelEntity { id = "n1", author_id = "author1", slug = "old-slug" };
            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1")).ReturnsAsync(novel);
            _currentUserMock.Setup(c => c.UserId).Returns("author1");
            _novelRepoMock.Setup(n => n.IsSlugExistsAsync("new-slug", It.IsAny<string?>())).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Novel.UpdateNovelResponse>(It.IsAny<NovelEntity>()))
                .Returns(new Shared.Contracts.Response.Novel.UpdateNovelResponse());

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity>());

            var command = new UpdateNovelCommand
            {
                NovelId = "n1",
                Title = "New Title",
                Slug = "new-slug",
                Description = "New description",
                IsPublic = true,
                Status = NovelStatus.Completed
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Novel Updated Successfullly");
            _novelRepoMock.Verify(n => n.UpdateNovelAsync(It.IsAny<NovelEntity>()), Times.Once);
        }
    }
}
