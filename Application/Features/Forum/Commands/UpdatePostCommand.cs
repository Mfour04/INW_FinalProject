using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Forum.Commands
{
    public class UpdatePostCommand : IRequest<ApiResponse>
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; }
        public List<IFormFile>? Images { get; set; }
        public string? RemovedImageUrls { get; set; } 
    }

    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, ApiResponse>
    {
        private readonly IForumPostRepository _postRepo;
        private readonly ICloudDinaryService _cloudDinaryService;

        public UpdatePostCommandHandler(IForumPostRepository postRepo, ICloudDinaryService cloudDinaryService)
        {
            _postRepo = postRepo;
            _cloudDinaryService = cloudDinaryService;
        }

        public async Task<ApiResponse> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return Fail("Nội dung không được để trống.");

            var post = await _postRepo.GetByIdAsync(request.Id);
            if (post == null)
                return Fail("Không tìm thấy bài đăng.");

            if (post.user_id != request.UserId)
                return Fail("Bạn không có quyền chỉnh sửa bài đăng này.");

            // Handle image updates
            var currentImages = post.img_urls ?? new List<string>();
            var updatedImages = new List<string>(currentImages);

            // Remove images that were marked for deletion
            if (!string.IsNullOrWhiteSpace(request.RemovedImageUrls))
            {
                var removedUrls = request.RemovedImageUrls.Split(',').ToList();
                
                updatedImages = updatedImages.Where(img => !removedUrls.Contains(img)).ToList();
            }

            // Upload new images
            if (request.Images != null && request.Images.Any())
            {
                var newImages = await _cloudDinaryService.UploadMultipleImagesAsync(request.Images, CloudFolders.Forums);
                updatedImages.AddRange(newImages);
            }

            ForumPostEntity updated = new()
            {
                content = request.Content,
                img_urls = updatedImages
            };

            var success = await _postRepo.UpdateAsync(request.Id, updated);
            
            if (!success)
                return Fail("Cập nhật bài đăng thất bại.");

            // Verify the update by fetching the post again
            var updatedPost = await _postRepo.GetByIdAsync(request.Id);

            return new ApiResponse
            {
                Success = true,
                Message = "Cập nhật bài đăng thành công."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
