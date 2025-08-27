using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Novel.Commands
{
    public class UpdateHideNovelCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public bool IsPublic { get; set; }
    }

    public class HideNovelCommandHandler : IRequestHandler<UpdateHideNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IChapterRepository _chapterRepository;
        public HideNovelCommandHandler(
        INovelRepository novelRepository,
        ICurrentUserService currentUserService,
        IChapterRepository chapterRepository
        )
        {
            _novelRepository = novelRepository;
            _currentUserService = currentUserService;
            _chapterRepository = chapterRepository;
        }
        public async Task<ApiResponse> Handle(UpdateHideNovelCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return new ApiResponse { Success = false, Message = "Chưa xác thực" };
            }
            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
            {
                return new ApiResponse { Success = false, Message = "Không tìm thấy truyện" };
            }
            if (novel.author_id != userId)
            {
                return new ApiResponse { Success = false, Message = "Bị cấm: Bạn không phải là tác giả của truyện này." };
            }
            await _novelRepository.UpdateHideNovelAsync(request.NovelId, request.IsPublic);
            await _chapterRepository.UpdateHideAllChaptersByNovelIdAsync(request.NovelId, request.IsPublic);

            var action = request.IsPublic ? "hiển thị" : "ẩn";
            return new ApiResponse
            {
                Success = true,
                Message = $"Truyện đã được {action} thành công và người dùng liên quan đã được thông báo."
            };
        }
    }
}
