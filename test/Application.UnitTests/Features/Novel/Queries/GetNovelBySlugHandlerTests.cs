using Application.Features.Novel.Queries;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Novel;

namespace Application.UnitTests.Features.Novel.Queries
{
    public class GetNovelBySlugHandlerTests
    {
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<IChapterRepository> _chapterRepoMock;
        private readonly Mock<IPurchaserRepository> _purchaserRepoMock;
        private readonly Mock<ITagRepository> _tagRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly GetNovelBySlugHandler _handler;

        public GetNovelBySlugHandlerTests()
        {
            _novelRepoMock = new Mock<INovelRepository>();
            _chapterRepoMock = new Mock<IChapterRepository>();
            _purchaserRepoMock = new Mock<IPurchaserRepository>();
            _tagRepoMock = new Mock<ITagRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _handler = new GetNovelBySlugHandler(
                _novelRepoMock.Object,
                _chapterRepoMock.Object,
                _purchaserRepoMock.Object,
                _tagRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object,
                _currentUserMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Not_Found()
        {
            _novelRepoMock.Setup(n => n.GetBySlugAsync("slug-1"))
                .ReturnsAsync((NovelEntity?)null);

            var result = await _handler.Handle(new GetNovelBySlug { SlugName = "slug-1" }, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Truyện không tồn tại.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Not_Public_And_User_Has_No_Access()
        {
            var novel = new NovelEntity
            {
                id = "n1",
                author_id = "a1",
                is_public = false,
                is_paid = true,
                tags = new List<string>()
            };

            _novelRepoMock.Setup(n => n.GetBySlugAsync("slug-1")).ReturnsAsync(novel);
            _currentUserMock.Setup(c => c.UserId).Returns("u1");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(false);

            _purchaserRepoMock.Setup(p => p.HasPurchasedFullAsync("u1", "n1")).ReturnsAsync(false);
            _chapterRepoMock.Setup(c => c.GetIdsByNovelIdAsync("n1")).ReturnsAsync(new List<string>());
            _purchaserRepoMock.Setup(p => p.HasAnyPurchasedChapterAsync("u1", "n1", It.IsAny<List<string>>()))
                .ReturnsAsync(false);

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity>());

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity>());

            _mapperMock.Setup(m => m.Map<NovelResponse>(It.IsAny<NovelEntity>()))
                .Returns(new NovelResponse());

            var result = await _handler.Handle(new GetNovelBySlug { SlugName = "slug-1" }, CancellationToken.None);

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
                is_paid = false,
                tags = new List<string> { "t1" }
            };

            _novelRepoMock.Setup(n => n.GetBySlugAsync("slug-1")).ReturnsAsync(novel);

            _userRepoMock.Setup(u => u.GetUsersByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<UserEntity> { new UserEntity { id = "a1", displayname = "Author 1" } });

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity> { new TagEntity { id = "t1", name = "Tag 1" } });

            _chapterRepoMock.Setup(c => c.GetPagedByNovelIdAsync("n1", It.IsAny<ChapterFindCreterias>(), It.IsAny<List<SortCreterias>>()))
                .ReturnsAsync((new List<ChapterEntity> { new ChapterEntity { id = "c1", is_paid = false, is_public = true } }, 1, 1));

            _chapterRepoMock.Setup(c => c.GetIdsByNovelIdAsync("n1"))
                .ReturnsAsync(new List<string> { "c1" });

            _mapperMock.Setup(m => m.Map<NovelResponse>(novel))
                .Returns(new NovelResponse());

            _mapperMock.Setup(m => m.Map<List<ChapterResponse>>(It.IsAny<List<ChapterEntity>>()))
                .Returns(new List<ChapterResponse>());

            _currentUserMock.Setup(c => c.UserId).Returns("a1");
            _currentUserMock.Setup(c => c.IsAdmin()).Returns(false);

            var result = await _handler.Handle(new GetNovelBySlug { SlugName = "slug-1" }, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Data.ShouldNotBeNull();

            var isAccessFullProp = result.Data?.GetType().GetProperty("IsAccessFull");
            isAccessFullProp.ShouldNotBeNull("Property 'IsAccessFull' not found on result.Data.");

            var isAccessFull = isAccessFullProp.GetValue(result.Data);
            isAccessFull.ShouldBeOfType<bool>();
            ((bool)isAccessFull).ShouldBeTrue();
        }
    }
}
