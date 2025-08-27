using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Chapter.Commands
{
    public class DeleteChapterCommand: IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
    }
    public class DeteleChapterHandler : IRequestHandler<DeleteChapterCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;
        public DeteleChapterHandler(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }
        public async Task<ApiResponse> Handle(DeleteChapterCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không tìm thấy chương"
                };
            }
            var deleted= await _chapterRepository.DeleteAsync(request.ChapterId);
            if (chapter.is_public && !chapter.is_draft)
            {
                await _chapterRepository.RenumberAsync(chapter.novel_id);
                await _novelRepository.UpdateTotalChaptersAsync(chapter.novel_id);
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Xóa chương thành công",
                Data = deleted
            };
        }
    }
}
