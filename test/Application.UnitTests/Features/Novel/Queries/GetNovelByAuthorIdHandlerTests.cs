using Application.Features.Novel.Queries;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Newtonsoft.Json.Linq;
using Shouldly;

namespace Application.UnitTests.Features.Novel.Queries
{
    public class GetNovelByAuthorIdHandlerTests
    {
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<ITagRepository> _tagRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly GetNovelByAuthorIdHandler _handler;

        public GetNovelByAuthorIdHandlerTests()
        {
            _novelRepoMock = new Mock<INovelRepository>();
            _tagRepoMock = new Mock<ITagRepository>();
            _mapperMock = new Mock<IMapper>();
            _userRepoMock = new Mock<IUserRepository>();
            _handler = new GetNovelByAuthorIdHandler(
                _novelRepoMock.Object,
                _mapperMock.Object,
                _tagRepoMock.Object,
                _userRepoMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Return_False_When_Author_Not_Found()
        {
            _novelRepoMock.Setup(n => n.GetNovelByAuthorId("a1"))
                .ReturnsAsync((List<NovelEntity>?)null);

            var result = await _handler.Handle(new GetNovelByAuthorId { AuthorId = "a1" }, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("AuthorId not found");
        }

        [Fact]
        public async Task Handle_Should_Return_Novels_Successfully()
        {
            var novels = new List<NovelEntity>
            {
                new NovelEntity { id = "n1", author_id = "a1", tags = new List<string> { "t1" } }
            };

            _novelRepoMock.Setup(n => n.GetNovelByAuthorId("a1")).ReturnsAsync(novels);
            _mapperMock.Setup(m => m.Map<List<Shared.Contracts.Response.Novel.NovelResponse>>(novels))
                .Returns(new List<Shared.Contracts.Response.Novel.NovelResponse>
                {
            new Shared.Contracts.Response.Novel.NovelResponse { NovelId = "n1", TotalViews = 10, CommentCount = 5 }
                });
            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity> { new UserEntity { id = "a1", displayname = "Author 1" } });
            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity> { new TagEntity { id = "t1", name = "Tag 1" } });

            var result = await _handler.Handle(new GetNovelByAuthorId { AuthorId = "a1" }, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Get Novel By Novel By AuthorId Successfully");

            var json = JObject.FromObject(result.Data!);
            json["TotalNovelViews"]!.Value<int>().ShouldBe(10);
            json["TotalComments"]!.Value<int>().ShouldBe(5);
        }
    }
}
