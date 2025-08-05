using Application.Features.Forum.Commands;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shared.Contracts.Response.Forum;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Commands
{
    public class CreatePostCommentCommandHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IForumCommentRepository> _commentRepoMock;
        private readonly Mock<IForumPostRepository> _postRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly CreatePostCommentCommandHandler _handler;

        public CreatePostCommentCommandHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _commentRepoMock = new Mock<IForumCommentRepository>();
            _postRepoMock = new Mock<IForumPostRepository>();
            _userRepoMock = new Mock<IUserRepository>();

            _handler = new CreatePostCommentCommandHandler(
            _mapperMock.Object,
            _commentRepoMock.Object,
            _postRepoMock.Object,
            _userRepoMock.Object
            );
        }

        // ----------------- HAPPY PATH -----------------

        [Fact]
        public async Task Handle_Should_Create_Comment_Successfully()
        {
            // Arrange
            var command = new CreatePostCommentCommand
            {
                PostId = "post123",
                Content = "Test comment",
                UserId = "user123"
            };

            _postRepoMock.Setup(p => p.GetByIdAsync("post123"))
            .ReturnsAsync(new ForumPostEntity { id = "post123" });

            _commentRepoMock.Setup(c => c.CreateAsync(It.IsAny<ForumCommentEntity>()))
            .ReturnsAsync(new ForumCommentEntity());

            _mapperMock.Setup(m => m.Map<PostCommentCreatedResponse>(It.IsAny<ForumCommentEntity>()))
            .Returns(new PostCommentCreatedResponse());

            _postRepoMock.Setup(p => p.IncrementCommentsAsync("post123"))
            .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment created successfully.");
        }

        [Fact]
        public async Task Handle_Should_Create_Reply_Successfully()
        {
            // Arrange
            var command = new CreatePostCommentCommand
            {
                ParentCommentId = "parent123",
                Content = "Test reply",
                UserId = "user123"
            };

            _commentRepoMock.Setup(c => c.GetByIdAsync("parent123"))
            .ReturnsAsync(new ForumCommentEntity { id = "parent123", post_id = "post123" });

            _commentRepoMock.Setup(c => c.CreateAsync(It.IsAny<ForumCommentEntity>()))
            .ReturnsAsync(new ForumCommentEntity());

            _mapperMock.Setup(m => m.Map<PostCommentCreatedResponse>(It.IsAny<ForumCommentEntity>()))
            .Returns(new PostCommentCreatedResponse());

            _commentRepoMock.Setup(c => c.IncrementReplyCountAsync("parent123"))
            .ReturnsAsync(true);

            _postRepoMock.Setup(p => p.IncrementCommentsAsync("post123"))
            .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Reply created successfully.");
        }

        // ----------------- FAILURE CASES -----------------

        [Fact]
        public async Task Handle_Should_Fail_When_Post_Not_Found()
        {
            var command = new CreatePostCommentCommand
            {
                PostId = "invalid_post",
                Content = "Test",
                UserId = "user123"
            };

            _postRepoMock.Setup(p => p.GetByIdAsync("invalid_post"))
            .ReturnsAsync((ForumPostEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Post does not exist.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Parent_Comment_Not_Found()
        {
            var command = new CreatePostCommentCommand
            {
                ParentCommentId = "invalid_parent",
                Content = "Reply",
                UserId = "user123"
            };

            _commentRepoMock.Setup(c => c.GetByIdAsync("invalid_parent"))
            .ReturnsAsync((ForumCommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Parent comment not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Parent_Is_Reply()
        {
            var command = new CreatePostCommentCommand
            {
                ParentCommentId = "reply123",
                Content = "Reply",
                UserId = "user123"
            };

            _commentRepoMock.Setup(c => c.GetByIdAsync("reply123"))
            .ReturnsAsync(new ForumCommentEntity { parent_comment_id = "grandparent123" });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Only 1-level replies are allowed.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Both_PostId_And_ParentCommentId_Provided()
        {
            var command = new CreatePostCommentCommand
            {
                PostId = "post123",
                ParentCommentId = "parent123",
                Content = "Test",
                UserId = "user123"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Either PostId or ParentCommentId must be provided, but not both.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_CreateAsync_Returns_Null()
        {
            var command = new CreatePostCommentCommand
            {
                PostId = "post123",
                Content = "Test comment",
                UserId = "user123"
            };

            _postRepoMock.Setup(p => p.GetByIdAsync("post123"))
            .ReturnsAsync(new ForumPostEntity { id = "post123" });

            _commentRepoMock.Setup(c => c.CreateAsync(It.IsAny<ForumCommentEntity>()))
            .ReturnsAsync((ForumCommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to create comment.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_IncrementCommentCount_Fails()
        {
            var command = new CreatePostCommentCommand
            {
                PostId = "post123",
                Content = "Test comment",
                UserId = "user123"
            };

            _postRepoMock.Setup(p => p.GetByIdAsync("post123"))
            .ReturnsAsync(new ForumPostEntity { id = "post123" });

            _commentRepoMock.Setup(c => c.CreateAsync(It.IsAny<ForumCommentEntity>()))
            .ReturnsAsync(new ForumCommentEntity());

            _mapperMock.Setup(m => m.Map<PostCommentCreatedResponse>(It.IsAny<ForumCommentEntity>()))
            .Returns(new PostCommentCreatedResponse());

            _postRepoMock.Setup(p => p.IncrementCommentsAsync("post123"))
            .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to update post comment count.");
        }
    }
}
