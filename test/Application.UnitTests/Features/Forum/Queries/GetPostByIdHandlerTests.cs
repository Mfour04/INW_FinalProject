using Application.Features.Forum.Queries;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Queries
{
    public class GetPostByIdHandlerTests
    {
        private readonly Mock<IForumPostRepository> _postRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPostByIdHandler _handler;

        public GetPostByIdHandlerTests()
        {
            _postRepoMock = new Mock<IForumPostRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetPostByIdHandler(
                _postRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Post_Not_Found()
        {
            var query = new GetPostById { Id = "notfound" };

            _postRepoMock.Setup(p => p.GetByIdAsync("notfound"))
                .ReturnsAsync((ForumPostEntity?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("No forum posts found.");
        }

        [Fact]
        public async Task Handle_Should_Return_Post_Without_User()
        {
            var query = new GetPostById { Id = "p1" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity { id = "p1", user_id = "u1" });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostResponse>(It.IsAny<ForumPostEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostResponse());

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Post retrieved successfully.");
            result.Data.ShouldNotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_Post_With_User()
        {
            var query = new GetPostById { Id = "p1" };

            _postRepoMock.Setup(p => p.GetByIdAsync("p1"))
                .ReturnsAsync(new ForumPostEntity { id = "p1", user_id = "u1" });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostResponse>(It.IsAny<ForumPostEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostResponse());

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync(new UserEntity { id = "u1", username = "user1", avata_url = "avatar1" });

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Post retrieved successfully.");
            result.Data.ShouldNotBeNull();
        }
    }
}
