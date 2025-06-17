using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Chapter.Command
{
    public class DeleteChapterCommand: IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
    }
    public class DeteleChapterHandler : IRequestHandler<DeleteChapterCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        public DeteleChapterHandler(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }
        public async Task<ApiResponse> Handle(DeleteChapterCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _chapterRepository.GetByChapterIdAsync(request.ChapterId);
            if (chapter == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Chapter not found"
                };
            }
            var deleted= await _chapterRepository.DeleteChapterAsync(request.ChapterId);
            if (chapter.is_public && !chapter.is_draft)
            {
                await _chapterRepository.RenumberChaptersAsync(chapter.novel_id);
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Chapter Deleted Succuessfully",
                Data = deleted
            };
        }
    }
}
