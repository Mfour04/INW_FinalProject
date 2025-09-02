using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Chapter.Commands
{
    public class UpdateHideChapterStatusCommand: IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
        public bool IsPublic { get; set; }
    }
    public class UpdateHideChapterStatusHandler : IRequestHandler<UpdateHideChapterStatusCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INovelRepository _novelRepository;
        public UpdateHideChapterStatusHandler(IChapterRepository chapterRepository
            , ICurrentUserService currentUserService, INovelRepository novelRepository)
        {
            _chapterRepository = chapterRepository;
            _currentUserService = currentUserService;
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(UpdateHideChapterStatusCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return new ApiResponse { Success = false, Message = "Chưa xác thực" };
            }

            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                return new ApiResponse { Success = false, Message = "Không tìm thấy chương" };
            }
            var novel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
            if (novel == null)
            {
                return new ApiResponse { Success = false, Message = "Không tìm thấy truyện" };
            }
            if (novel.author_id == null)
            {
                return new ApiResponse { Success = false, Message = "Không tìm thấy tác giả truyện" };
            }

            // Kiểm tra quyền
            if (novel.author_id != userId)
            {
                return new ApiResponse { Success = false, Message = "Bạn không có quyền cập nhật chương này" };
            }

            await _chapterRepository.UpdateHideChapterStatus(request.ChapterId, request.IsPublic);

            var action = request.IsPublic ? "hiển thị" : "ẩn";
            return new ApiResponse
            {
                Success = true,
                Message = $"Chương đã được {action} thành công và người dùng liên quan đã được thông báo."
            };
        }
    }
}
