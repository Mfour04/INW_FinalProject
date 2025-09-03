using Application.Features.Forum.Queries;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Queries
{
    public class GetPostCommentByIdHandlerTests
    {
        private readonly Mock<IForumCommentRepository> _commentRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPostCommentByIdHandler _handler;

        public GetPostCommentByIdHandlerTests()
        {
            _commentRepoMock = new Mock<IForumCommentRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetPostCommentByIdHandler(
                _commentRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Comment_Not_Found()
        {
            var query = new GetPostCommentById { Id = "notfound" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("notfound"))
                .ReturnsAsync((ForumCommentEntity?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("No forum comment found.");
        }

        [Fact]
        public async Task Handle_Should_Return_Comment_Without_User()
        {
            var query = new GetPostCommentById { Id = "c1" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("c1"))
                .ReturnsAsync(new ForumCommentEntity { id = "c1", user_id = "u1" });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostCommentResponse>(It.IsAny<ForumCommentEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostCommentResponse());

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment retrieved successfully.");
            result.Data.ShouldNotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_Comment_With_User()
        {
            var query = new GetPostCommentById { Id = "c1" };

            _commentRepoMock.Setup(c => c.GetByIdAsync("c1"))
                .ReturnsAsync(new ForumCommentEntity { id = "c1", user_id = "u1" });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostCommentResponse>(It.IsAny<ForumCommentEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostCommentResponse());

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync(new UserEntity { id = "u1", username = "user1", displayname = "User One", avata_url = "avatar1" });

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment retrieved successfully.");
            result.Data.ShouldNotBeNull();
        }
    }
}
