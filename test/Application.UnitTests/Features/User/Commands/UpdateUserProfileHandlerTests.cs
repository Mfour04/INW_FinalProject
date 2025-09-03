using Application.Features.User.Feature;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;
using Shouldly;

namespace Application.UnitTests.Features.User.Commands
{
    public class UpdateUserProfileHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ICloudDinaryService> _cloudServiceMock = new();
        private readonly Mock<IOpenAIService> _openAIServiceMock = new();
        private readonly Mock<IOpenAIRepository> _openAIRepoMock = new();
        private readonly Mock<ITagRepository> _tagRepoMock = new();
        private readonly UpdateUserProfileHandler _handler;

        public UpdateUserProfileHandlerTests()
        {
            _handler = new UpdateUserProfileHandler(
                _userRepoMock.Object,
                _mapperMock.Object,
                _cloudServiceMock.Object,
                _openAIServiceMock.Object,
                _openAIRepoMock.Object,
                _tagRepoMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Found()
        {
            var command = new UpdateUserProfileCommand
            {
                UserId = "user-1"
            };

            _userRepoMock.Setup(r => r.GetById("user-1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User not found.");
        }

        [Fact]
        public async Task Handle_Should_Succeed_Without_Changing_FavouriteTypes()
        {
            var command = new UpdateUserProfileCommand
            {
                UserId = "user-1",
                DisplayName = "New Name",
                Bio = "New bio",
                BadgeId = new List<string> { "badge-1" },
                FavouriteType = new List<string> { "tag-1", "tag-2" }
            };

            var user = new UserEntity
            {
                id = "user-1",
                displayname = "Old Name",
                favourite_type = new List<string> { "tag-1", "tag-2" }
            };

            _userRepoMock.Setup(r => r.GetById("user-1")).ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<UpdateUserProfileReponse>(It.IsAny<UserEntity>()))
                .Returns(new UpdateUserProfileReponse { DisplayName = "New Name" });

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(command.FavouriteType))
                .ReturnsAsync(new List<TagEntity>
                {
                    new TagEntity { id = "tag-1", name = "Fantasy" },
                    new TagEntity { id = "tag-2", name = "Action" }
                });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Profile updated successfully.");
            result.Data.ShouldBeOfType<UpdateUserProfileReponse>();

            _openAIServiceMock.Verify(s => s.GetEmbeddingAsync(It.IsAny<List<string>>()), Times.Never);
            _openAIRepoMock.Verify(r => r.SaveUserEmbeddingAsync(It.IsAny<string>(), It.IsAny<List<float>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Recalculate_Embedding_When_FavouriteTypes_Changed()
        {
            var command = new UpdateUserProfileCommand
            {
                UserId = "user-1",
                DisplayName = "New Name",
                FavouriteType = new List<string> { "tag-1", "tag-3" }
            };

            var user = new UserEntity
            {
                id = "user-1",
                displayname = "Old Name",
                favourite_type = new List<string> { "tag-1", "tag-2" }
            };

            _userRepoMock.Setup(r => r.GetById("user-1")).ReturnsAsync(user);

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(command.FavouriteType))
                .ReturnsAsync(new List<TagEntity>
                {
                    new TagEntity { id = "tag-1", name = "Fantasy" },
                    new TagEntity { id = "tag-3", name = "Sci-fi" }
                });

            _openAIServiceMock.Setup(o => o.GetEmbeddingAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<List<float>> { new List<float> { 0.1f, 0.2f } });

            _mapperMock.Setup(m => m.Map<UpdateUserProfileReponse>(It.IsAny<UserEntity>()))
                .Returns(new UpdateUserProfileReponse());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();

            _openAIServiceMock.Verify(o => o.GetEmbeddingAsync(It.IsAny<List<string>>()), Times.Once);
            _openAIRepoMock.Verify(o => o.SaveUserEmbeddingAsync("user-1", It.IsAny<List<float>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Upload_Avatar_And_Cover_When_Provided()
        {
            var fakeFile = new Mock<IFormFile>();

            var command = new UpdateUserProfileCommand
            {
                UserId = "user-1",
                DisplayName = "Test",
                Bio = "Test bio",
                AvataUrl = fakeFile.Object,
                CoverUrl = fakeFile.Object
            };

            var user = new UserEntity { id = "user-1" };

            _userRepoMock.Setup(r => r.GetById("user-1")).ReturnsAsync(user);
            _cloudServiceMock.Setup(c => c.UploadImagesAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("http://image-url");

            _mapperMock.Setup(m => m.Map<UpdateUserProfileReponse>(It.IsAny<UserEntity>()))
                .Returns(new UpdateUserProfileReponse());

            _tagRepoMock.Setup(t => t.GetTagsByIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<TagEntity>());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();

            _cloudServiceMock.Verify(c =>
                c.UploadImagesAsync(It.IsAny<IFormFile>(), CloudFolders.Users), Times.Exactly(2)); // ✅ Gọi 2 lần
        }
    }
}
