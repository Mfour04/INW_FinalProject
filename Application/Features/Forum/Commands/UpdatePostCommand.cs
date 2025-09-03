using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class UpdatePostCommand : IRequest<ApiResponse>
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<string>? KeepUrls { get; set; }
        public List<IFormFile>? NewImages { get; set; }
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
            if (string.IsNullOrWhiteSpace(request.Id))
                return Fail("Thiếu Id bài đăng.");

            var post = await _postRepo.GetByIdAsync(request.Id);
            if (post == null)
                return Fail("Không tìm thấy bài đăng.");
            if (post.user_id != request.UserId)
                return Fail("Bạn không có quyền chỉnh sửa bài đăng này.");

            var valid = ValidateUpdate(request, post);
            if (!valid.IsValid) return valid.FailResponse!;

            var oldUrls = post.img_urls ?? new List<string>();
            var keepUrls = request.KeepUrls?.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList() ?? new List<string>();

            var toDelete = oldUrls.Except(keepUrls).ToList();
            if (toDelete.Count > 0)
            {
                await _cloudDinaryService.DeleteMultipleImagesAsync(toDelete);
            }

            var newUploaded = new List<string>();
            if (request.NewImages != null && request.NewImages.Count > 0)
            {
                newUploaded = await _cloudDinaryService.UploadMultipleImagesAsync(request.NewImages, CloudFolders.Forums);
                if (newUploaded == null || newUploaded.Count == 0)
                    return Fail("Tải ảnh lên thất bại.");
            }

            var finalUrls = new List<string>(keepUrls.Count + newUploaded.Count);
            finalUrls.AddRange(keepUrls);
            finalUrls.AddRange(newUploaded);

            var updated = new ForumPostEntity
            {
                content = (request.Content ?? string.Empty).Trim(),
                img_urls = finalUrls
            };

            var success = await _postRepo.UpdateAsync(request.Id!, updated);
            if (!success) return Fail("Cập nhật bài đăng thất bại.");

            return new ApiResponse { Success = true, Message = "Cập nhật bài đăng thành công." };
        }

        /// <summary>
        /// Không cho phép bài đăng trở thành "rỗng": sau update phải còn content hoặc ít nhất một hình ảnh.
        /// </summary>
        private (bool IsValid, ApiResponse? FailResponse) ValidateUpdate(UpdatePostCommand request, ForumPostEntity post)
        {
            var newContent = (request.Content ?? string.Empty).Trim();
            bool hasContentAfter = !string.IsNullOrWhiteSpace(newContent);

            var keepUrls = request.KeepUrls?.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList() ?? new List<string>();
            bool hasNew = request.NewImages != null && request.NewImages.Count > 0;

            bool willHaveImages = keepUrls.Any() || hasNew;

            if (!hasContentAfter && !willHaveImages)
            {
                return (false, new ApiResponse
                {
                    Success = false,
                    Message = "Bài viết phải còn nội dung hoặc ít nhất một hình ảnh."
                });
            }

            return (true, null);
        }

        private static ApiResponse Fail(string message) => new() { Success = false, Message = message };
    }
}
