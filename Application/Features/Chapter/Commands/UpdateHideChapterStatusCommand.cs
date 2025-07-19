using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return new ApiResponse { Success = false, Message = "Unauthorized" };
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
            if (novel.author_id == null)
            {
                return new ApiResponse { Success = false, Message = "Novel author not found" };
            }

            // Kiểm tra quyền
            if (novel.author_id != userId)
            {
                return new ApiResponse { Success = false, Message = "You do not have permission to update this chapter" };
            }

            await _chapterRepository.UpdateHideChapterStatus(request.ChapterId, request.IsPublic);

            var action = request.IsPublic ? "unhidden" : "hidden";
            return new ApiResponse
            {
                Success = true,
                Message = $"Novel has been {action} successfully and affected users have been notified."
            };
        }   
    }
}
