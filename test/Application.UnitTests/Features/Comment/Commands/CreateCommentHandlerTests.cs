using Application.Features.Comment.Commands;
using Application.Features.Notification.Commands;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Moq;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using Shouldly;

namespace Application.UnitTests.Features.Comment.Commands
{
    public class CreateCommentHandlerTests
    {
        private readonly Mock<ICommentRepository> _commentRepoMock = new();
        private readonly Mock<IChapterRepository> _chapterRepoMock = new();
        private readonly Mock<INovelRepository> _novelRepoMock = new();
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ICommentSpamGuard> _spamGuardMock = new();

        private readonly CreateCommentCommandHandler _handler;

        public CreateCommentHandlerTests()
        {
            _handler = new CreateCommentCommandHandler(
                _commentRepoMock.Object,
                _chapterRepoMock.Object,
                _novelRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object,
                _mediatorMock.Object,
                _spamGuardMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Content_Is_Empty()
        {
            var command = new CreateCommentCommand { Content = "   " };
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Content is required.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_ParentComment_NotFound()
        {
            var command = new CreateCommentCommand
            {
                Content = "Reply",
                ParentCommentId = "invalid-parent"
            };

            _commentRepoMock.Setup(r => r.GetByIdAsync("invalid-parent"))
                .ReturnsAsync((CommentEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Parent comment not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_NovelId_And_ChapterId_Are_Empty()
        {
            var command = new CreateCommentCommand
            {
                Content = "Comment gốc"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Either NovelId or ChapterId must be provided.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Chapter_NotFound()
        {
            var command = new CreateCommentCommand
            {
                Content = "Comment chương",
                ChapterId = "chapter1"
            };

            _chapterRepoMock.Setup(r => r.GetByIdAsync("chapter1"))
                .ReturnsAsync((ChapterEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Chapter not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_NotFound_From_Chapter()
        {
            var command = new CreateCommentCommand
            {
                Content = "Comment chương",
                ChapterId = "chapter1"
            };

            _chapterRepoMock.Setup(r => r.GetByIdAsync("chapter1"))
                .ReturnsAsync(new ChapterEntity { id = "chapter1", novel_id = "novel-x" });

            _novelRepoMock.Setup(r => r.GetByNovelIdAsync("novel-x"))
                .ReturnsAsync((NovelEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Novel not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_NotFound_Direct()
        {
            var command = new CreateCommentCommand
            {
                Content = "Comment truyện",
                NovelId = "novel1"
            };

            _novelRepoMock.Setup(r => r.GetByNovelIdAsync("novel1"))
                .ReturnsAsync((NovelEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Novel not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_SpamDetected()
        {
            var command = new CreateCommentCommand
            {
                Content = "Spam comment",
                NovelId = "novel1",
                UserId = "user1"
            };

            _novelRepoMock.Setup(r => r.GetByNovelIdAsync("novel1"))
                .ReturnsAsync(new NovelEntity { id = "novel1" });

            _spamGuardMock.Setup(s => s.CheckSpamAsync("user1", "novel1", null, "Spam comment"))
                .ReturnsAsync(new ApiResponse { Success = false, Message = "Spam detected." });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Spam detected.");
        }

        [Fact]
        public async Task Handle_Should_Create_Comment_For_Novel()
        {
            var command = new CreateCommentCommand
            {
                Content = "This is a test comment.",
                NovelId = "novel1",
                UserId = "user1"
            };

            _novelRepoMock.Setup(r => r.GetByNovelIdAsync("novel1"))
                .ReturnsAsync(new NovelEntity { id = "novel1" });

            _spamGuardMock.Setup(s => s.CheckSpamAsync("user1", "novel1", null, command.Content))
                .ReturnsAsync((ApiResponse?)null);

            _mediatorMock.Setup(m => m.Send(It.IsAny<SendNotificationToUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse { Success = true });

            _userRepoMock.Setup(u => u.GetById("user1"))
                .ReturnsAsync(new UserEntity { id = "user1", username = "user", displayname = "User", avata_url = "avatar.png" });

            _mapperMock.Setup(m => m.Map<CommentCreatedResponse>(It.IsAny<CommentEntity>()))
                .Returns(new CommentCreatedResponse());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Comment created successfully and SignalR sent.");
            _commentRepoMock.Verify(c => c.CreateAsync(It.IsAny<CommentEntity>()), Times.Once);
        }
    }
}
