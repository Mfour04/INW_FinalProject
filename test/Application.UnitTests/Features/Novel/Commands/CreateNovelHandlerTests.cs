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
    public class CreateNovelHandlerTests
    {
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ITagRepository> _tagRepoMock;
        private readonly Mock<ICloudDinaryService> _cloudServiceMock;
        private readonly Mock<IOpenAIService> _openAIServiceMock;
        private readonly Mock<IOpenAIRepository> _openAIRepoMock;
        private readonly CreateNovelHandler _handler;

        public CreateNovelHandlerTests()
        {
            _novelRepoMock = new Mock<INovelRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _tagRepoMock = new Mock<ITagRepository>();
            _cloudServiceMock = new Mock<ICloudDinaryService>();
            _openAIServiceMock = new Mock<IOpenAIService>();
            _openAIRepoMock = new Mock<IOpenAIRepository>();

            _handler = new CreateNovelHandler(
                _novelRepoMock.Object,
                _mapperMock.Object,
                _userRepoMock.Object,
                _tagRepoMock.Object,
                _cloudServiceMock.Object,
                _openAIServiceMock.Object,
                _openAIRepoMock.Object
            );
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Fail_When_Author_Not_Found()
        {
            var command = new CreateNovelCommand
            {
                AuthorId = "a1",
                Slug = "slug-1",
                Title = "Novel"
            };

            _userRepoMock.Setup(u => u.GetById("a1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Author not found");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Slug_Is_Empty()
        {
            var command = new CreateNovelCommand
            {
                AuthorId = "a1",
                Slug = "",
                Title = "Novel"
            };

            _userRepoMock.Setup(u => u.GetById("a1"))
                .ReturnsAsync(new UserEntity { id = "a1" });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Slug is required.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Slug_Exists()
        {
            var command = new CreateNovelCommand
            {
                AuthorId = "a1",
                Slug = "slug-1",
                Title = "Novel"
            };

            _userRepoMock.Setup(u => u.GetById("a1"))
                .ReturnsAsync(new UserEntity { id = "a1" });

            _novelRepoMock.Setup(n => n.IsSlugExistsAsync("slug-1", It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Slug already exists.");
        }

        // ---------- SUCCESS CASE ----------

        [Fact]
        public async Task Handle_Should_Create_Novel_Successfully()
        {
            var command = new CreateNovelCommand
            {
                AuthorId = "a1",
                Slug = "slug-1",
                Title = "Novel",
                Tags = new List<string> { "t1", "t2" },
                Status = NovelStatus.Completed
            };

            _userRepoMock.Setup(u => u.GetById("a1"))
                .ReturnsAsync(new UserEntity { id = "a1", displayname = "Author Name" });

            _novelRepoMock.Setup(n => n.IsSlugExistsAsync("slug-1", It.IsAny<string>()))
                .ReturnsAsync(false);

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(command.Tags))
                .ReturnsAsync(new List<TagEntity>
                {
                    new TagEntity { id = "t1", name = "Tag 1" },
                    new TagEntity { id = "t2", name = "Tag 2" }
                });

            _cloudServiceMock.Setup(c => c.UploadImagesAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("image-url");

            _openAIServiceMock.Setup(o => o.GetEmbeddingAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<List<float>> { new List<float> { 0.1f, 0.2f } });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Novel.CreateNovelResponse>(It.IsAny<NovelEntity>()))
                .Returns(new Shared.Contracts.Response.Novel.CreateNovelResponse());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Created Novel Successfully");
            _novelRepoMock.Verify(n => n.CreateNovelAsync(It.IsAny<NovelEntity>()), Times.Once);
            _openAIRepoMock.Verify(o => o.SaveNovelEmbeddingAsync(It.IsAny<string>(), It.IsAny<List<float>>()), Times.Once);
        }
    }
}
