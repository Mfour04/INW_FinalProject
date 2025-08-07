using Application.Features.Novel.Queries;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shared.Contracts.Response.Novel;
using Shouldly;

namespace Application.UnitTests.Features.Novel.Queries
{
    public class GetNovelByIdHandlerTests
    {
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<IChapterRepository> _chapterRepoMock;
        private readonly Mock<IPurchaserRepository> _purchaserRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ITagRepository> _tagRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly GetNovelByIdHandler _handler;

        public GetNovelByIdHandlerTests()
        {
            _novelRepoMock = new Mock<INovelRepository>();
            _chapterRepoMock = new Mock<IChapterRepository>();
            _purchaserRepoMock = new Mock<IPurchaserRepository>();
            _mapperMock = new Mock<IMapper>();
            _tagRepoMock = new Mock<ITagRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _handler = new GetNovelByIdHandler(
                _novelRepoMock.Object,
                _chapterRepoMock.Object,
                _purchaserRepoMock.Object,
                _mapperMock.Object,
                _tagRepoMock.Object,
                _userRepoMock.Object,
                _currentUserMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Not_Found()
        {
            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync((NovelEntity?)null);

            var result = await _handler.Handle(new GetNovelById { NovelId = "n1" }, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Truyện không tồn tại.");
        }

        [Fact]
        public async Task Handle_Should_Return_False_When_Novel_Not_Public_And_User_No_Access()
        {
            var novel = new NovelEntity
            {
                id = "n1",
                author_id = "a1",
                is_public = false,
                is_paid = true,
                tags = new List<string>()
            };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1")).ReturnsAsync(novel);

            _currentUserMock.Setup(c => c.UserId).Returns("user1");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(false);

            _purchaserRepoMock.Setup(p => p.HasPurchasedFullAsync("user1", "n1")).ReturnsAsync(false);

            _chapterRepoMock.Setup(c => c.GetIdsByNovelIdAsync("n1"))
                .ReturnsAsync(new List<string>());

            _purchaserRepoMock.Setup(p => p.HasAnyPurchasedChapterAsync("user1", "n1", It.IsAny<List<string>>()))
                .ReturnsAsync(false);

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity> {
                        new TagEntity { id = "tag1", name = "Adventure" }
                });

            _mapperMock.Setup(m => m.Map<NovelResponse>(It.IsAny<NovelEntity>()))
                .Returns(new NovelResponse());

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity>());

            var result = await _handler.Handle(new GetNovelById { NovelId = "n1" }, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Truyện này chưa được công khai.");
        }

        [Fact]
        public async Task Handle_Should_Return_Novel_Details_Successfully()
        {
            var novel = new NovelEntity
            {
                id = "n1",
                author_id = "a1",
                is_public = true,
                tags = new List<string> { "t1" }
            };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1")).ReturnsAsync(novel);

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity> { new UserEntity { id = "a1", displayname = "Author 1" } });

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity> { new TagEntity { id = "t1", name = "Tag 1" } });

            _chapterRepoMock.Setup(c => c.GetIdsByNovelIdAsync("n1"))
                .ReturnsAsync(new List<string> { "c1", "c2" });

            _chapterRepoMock.Setup(c => c.GetPagedByNovelIdAsync("n1", It.IsAny<ChapterFindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync((
                    new List<ChapterEntity>
                    {
                        new ChapterEntity { id = "c1", is_public = true, is_paid = false },
                        new ChapterEntity { id = "c2", is_public = true, is_paid = true }
                    },
                    2,
                    1
                ));

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Novel.NovelResponse>(novel))
                .Returns(new Shared.Contracts.Response.Novel.NovelResponse());

            _mapperMock.Setup(m => m.Map<List<Shared.Contracts.Response.Chapter.ChapterResponse>>(It.IsAny<List<ChapterEntity>>()))
                .Returns(new List<Shared.Contracts.Response.Chapter.ChapterResponse>());

            _currentUserMock.Setup(c => c.UserId).Returns("a1");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(false);

            var result = await _handler.Handle(new GetNovelById { NovelId = "n1" }, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("Bạn có thể truy cập");
        }
    }
}
