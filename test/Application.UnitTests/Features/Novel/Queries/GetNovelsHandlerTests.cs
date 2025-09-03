using Application.Features.Novel.Queries;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;
using Shared.Contracts.Response.Novel;
using Newtonsoft.Json;

namespace Application.UnitTests.Features.Novel.Queries
{
    public class GetNovelHandlerTests
    {
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ITagRepository> _tagRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IOpenAIRepository> _openAIRepoMock;
        private readonly Mock<IOpenAIService> _openAIServiceMock;
        private readonly GetNovelsHandler _handler;

        public GetNovelHandlerTests()
        {
            _novelRepoMock = new Mock<INovelRepository>();
            _mapperMock = new Mock<IMapper>();
            _tagRepoMock = new Mock<ITagRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _openAIRepoMock = new Mock<IOpenAIRepository>();
            _openAIServiceMock = new Mock<IOpenAIService>();

            _handler = new GetNovelsHandler(
                _novelRepoMock.Object,
                _mapperMock.Object,
                _tagRepoMock.Object,
                _userRepoMock.Object,
                _openAIRepoMock.Object,
                _openAIServiceMock.Object
            );
        }

        private class NovelResultData
        {
            public List<NovelResponse> Novels { get; set; }
            public int TotalNovels { get; set; }
            public int TotalPages { get; set; }
            public EmbeddingStatsData EmbeddingStats { get; set; }
        }

        private class EmbeddingStatsData
        {
            public int TotalRequested { get; set; }
            public int AlreadyExist { get; set; }
            public int NewlyEmbedded { get; set; }
            public List<string> NewlyEmbeddedIds { get; set; }
        }

        [Fact]
        public async Task Handle_Should_Return_False_When_No_Novels_Found()
        {
            _novelRepoMock.Setup(n => n.GetAllNovelAsync(It.IsAny<FindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync((new List<NovelEntity>(), 0));

            var query = new GetNovels { SearchTerm = "nonexistent" };
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("No novels found.");
        }

        [Fact]
        public async Task Handle_Should_Return_Novels_Successfully_With_Exact_Match()
        {
            var novelList = new List<NovelEntity>
            {
                new NovelEntity { id = "n1", author_id = "a1", tags = new List<string> { "t1" } }
            };

            _novelRepoMock.Setup(n => n.GetAllNovelAsync(It.IsAny<FindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync((novelList, 1));

            _mapperMock.Setup(m => m.Map<List<NovelResponse>>(novelList))
                .Returns(new List<NovelResponse>
                {
                    new NovelResponse { NovelId = "n1" }
                });

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity> { new UserEntity { id = "a1", displayname = "Author 1" } });

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity> { new TagEntity { id = "t1", name = "Tag 1" } });

            _openAIRepoMock.Setup(o => o.GetExistingNovelEmbeddingIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<string>());

            _openAIServiceMock.Setup(o => o.GetEmbeddingAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<List<float>> { new List<float> { 0.1f, 0.2f } });

            var result = await _handler.Handle(new GetNovels { SearchTerm = "exact" }, CancellationToken.None);

            var data = JsonConvert.DeserializeObject<NovelResultData>(
                JsonConvert.SerializeObject(result.Data));

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Retrieved novels successfully.");
            data.TotalNovels.ShouldBe(1);
            data.EmbeddingStats.NewlyEmbeddedIds.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Handle_Should_Fallback_To_Fuzzy_Search_When_Exact_Fails()
        {
            _novelRepoMock.SetupSequence(n => n.GetAllNovelAsync(It.IsAny<FindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync((new List<NovelEntity>(), 0))
                .ReturnsAsync((new List<NovelEntity>
                {
                    new NovelEntity { id = "n2", author_id = "a2", tags = new List<string>() }
                }, 1));

            _mapperMock.Setup(m => m.Map<List<NovelResponse>>(It.IsAny<List<NovelEntity>>()))
                .Returns(new List<NovelResponse>
                {
                    new NovelResponse { NovelId = "n2" }
                });

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity> { new UserEntity { id = "a2", displayname = "Author 2" } });

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity>());

            _openAIRepoMock.Setup(o => o.GetExistingNovelEmbeddingIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<string>());

            var result = await _handler.Handle(new GetNovels { SearchTerm = "fuzzy" }, CancellationToken.None);

            var data = JsonConvert.DeserializeObject<NovelResultData>(
                JsonConvert.SerializeObject(result.Data));

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Retrieved novels successfully.");
            data.TotalNovels.ShouldBe(1);
        }

        [Fact]
        public async Task Handle_Should_Not_Call_Embedding_When_All_Novels_Already_Have_Embedding()
        {
            var novelList = new List<NovelEntity>
            {
                new NovelEntity { id = "n3", author_id = "a3", tags = new List<string> { "t3" } }
            };

            _novelRepoMock.Setup(n => n.GetAllNovelAsync(It.IsAny<FindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync((novelList, 1));

            _mapperMock.Setup(m => m.Map<List<NovelResponse>>(novelList))
                .Returns(new List<NovelResponse>
                {
                    new NovelResponse { NovelId = "n3" }
                });

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity> { new UserEntity { id = "a3", displayname = "Author 3" } });

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity> { new TagEntity { id = "t3", name = "Tag 3" } });

            _openAIRepoMock.Setup(o => o.GetExistingNovelEmbeddingIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<string> { "n3" });

            var result = await _handler.Handle(new GetNovels { SearchTerm = "test" }, CancellationToken.None);

            var data = JsonConvert.DeserializeObject<NovelResultData>(
                JsonConvert.SerializeObject(result.Data));

            result.Success.ShouldBeTrue();
            data.EmbeddingStats.NewlyEmbedded.ShouldBe(0);
            _openAIServiceMock.Verify(o => o.GetEmbeddingAsync(It.IsAny<List<string>>()), Times.Never);
        }
    }
}
