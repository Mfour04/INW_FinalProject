using Application.Features.Forum.Commands;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Forum.Commands
{
    public class CreatePostCommandHandlerTests
    {
        private readonly Mock<IForumPostRepository> _postRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICloudDinaryService> _cloudServiceMock;
        private readonly CreatePostCommandHandler _handler;

        public CreatePostCommandHandlerTests()
        {
            _postRepoMock = new Mock<IForumPostRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _cloudServiceMock = new Mock<ICloudDinaryService>();

            _handler = new CreatePostCommandHandler(
                _postRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object,
                _cloudServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Content_Is_Empty()
        {
            var command = new CreatePostCommand { Content = "" };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Content cannot be empty.");
        }

        [Fact]
        public async Task Handle_Should_Create_Post_With_Images()
        {
            var command = new CreatePostCommand
            {
                UserId = "u1",
                Content = "New post",
                Images = new List<IFormFile> { new FormFile(null, 0, 0, "file", "file.jpg") }
            };

            _cloudServiceMock.Setup(c => c.UploadMultipleImagesAsync(It.IsAny<List<IFormFile>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "url1", "url2" });

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync(new UserEntity { id = "u1", username = "user1", avata_url = "avatar" });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostCreatedResponse>(It.IsAny<ForumPostEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostCreatedResponse());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Post created successfully.");
        }

        [Fact]
        public async Task Handle_Should_Create_Post_Without_Images()
        {
            var command = new CreatePostCommand
            {
                UserId = "u1",
                Content = "New post"
            };

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync(new UserEntity { id = "u1", username = "user1", avata_url = "avatar" });

            _mapperMock.Setup(m => m.Map<Shared.Contracts.Response.Forum.PostCreatedResponse>(It.IsAny<ForumPostEntity>()))
                .Returns(new Shared.Contracts.Response.Forum.PostCreatedResponse());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Post created successfully.");
        }
    }
}
