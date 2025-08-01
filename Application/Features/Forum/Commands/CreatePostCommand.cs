using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Forum;
using Shared.Helpers;

namespace Application.Features.Forum.Commands
{
    public class CreatePostCommand : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public string Content { get; set; }
        public List<IFormFile>? Images { get; set; }
    }

    public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, ApiResponse>
    {
        private readonly IForumPostRepository _postRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly ICloudDinaryService _cloudDinaryService;

        public CreatePostCommandHandler(IForumPostRepository postRepo, IUserRepository userRepo, IMapper mapper, ICloudDinaryService cloudDinaryService)
        {
            _userRepo = userRepo;
            _postRepo = postRepo;
            _mapper = mapper;
            _cloudDinaryService = cloudDinaryService;
        }

        public async Task<ApiResponse> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return new ApiResponse { Success = false, Message = "Content cannot be empty." };

            var postImages = new List<string>();
            if (request.Images != null && request.Images.Any())
            {
                postImages = await _cloudDinaryService.UploadMultipleImagesAsync(request.Images, CloudFolders.Forums);
            }

			ForumPostEntity newPost = new()
            {
                id = SystemHelper.RandomId(),
                user_id = request.UserId,
                content = request.Content,
				img_urls = postImages,
                like_count = 0,
                comment_count = 0,
                created_at = TimeHelper.NowTicks
            };

            await _postRepo.CreateAsync(newPost);
            var response = _mapper.Map<PostCreatedResponse>(newPost);

            var author = await _userRepo.GetById(request.UserId);
            response.Author = new BasePostResponse.PostAuthorResponse
            {
                Id = author.id,
                Username = author.username,
                Avatar = author.avata_url
            };

            return new ApiResponse
            {
                Success = true,
                Message = "Post created successfully.",
                Data = response
            };
        }
    }
}