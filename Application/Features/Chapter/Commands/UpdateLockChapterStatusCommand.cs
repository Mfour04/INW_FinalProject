using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Chapter.Commands
{
    public class UpdateLockChapterStatusCommand: IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
        public bool IsLocked { get; set; }
    }
    public class UpdateLockChapterHandler : IRequestHandler<UpdateLockChapterStatusCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterHelperService _chapterHelperService;
        public UpdateLockChapterHandler(IChapterRepository chapterRepository
            , ICurrentUserService currentUserService, INovelRepository novelRepository, IChapterHelperService chapterHelperService)
        {
            _chapterRepository = chapterRepository;
            _currentUserService = currentUserService;
            _novelRepository = novelRepository;
            _chapterHelperService = chapterHelperService;
        }
        public async Task<ApiResponse> Handle(UpdateLockChapterStatusCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return new ApiResponse { Success = false, Message = "Unauthorized" };
            }
            if (!_currentUserService.IsAdmin())
            {
                return new ApiResponse { Success = false, Message = "Forbidden: Admin role required" };
            }
            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                return new ApiResponse { Success = false, Message = "Chapter not found" };
            }
            var novel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
            if (novel == null)
            {
                return new ApiResponse { Success = false, Message = "Novel not found" };
            }

            await _chapterRepository.UpdateLockChapterStatus(request.ChapterId, request.IsLocked);
            var action = request.IsLocked ? "locked" : "unlocked";
            return new ApiResponse
            {
                Success = true,
                Message = $"Chapter has been {action} successfully and affected users have been notified."
            };
        }
    }
}
