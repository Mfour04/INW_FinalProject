using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Chapter.Command
{
    public class UpdateChapterCommand: IRequest<ApiResponse>
    {
        public UpdateChapterResponse UpdateChapter { get; set; }
    }
    public class UpdateChapterHandler : IRequestHandler<UpdateChapterCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMapper _mapper;

        public UpdateChapterHandler(IChapterRepository chapterRepository, IMapper mapper)
        {
            _chapterRepository = chapterRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(UpdateChapterCommand request, CancellationToken cancellationToken)
        {
            var input = request.UpdateChapter;
            var chapter = await _chapterRepository.GetByChapterIdAsync(input.ChapterId);
            if (chapter == null)
                return new ApiResponse { Success = false, Message = "Chapter not found" };

            chapter.title = input.Title ?? chapter.title;
            chapter.content = input.Content ?? chapter.content;
            chapter.chapter_number = input.ChapterNumber ?? chapter.chapter_number;
            chapter.is_paid = input.IsPaid ?? chapter.is_paid;
            chapter.price = input.Price ?? chapter.price;
            chapter.updated_at = DateTime.UtcNow.Ticks;
            await _chapterRepository.UpdateChapterAsync(chapter);
            var response = _mapper.Map<UpdateChapterResponse>(chapter);

            return new ApiResponse
            {
                Success = true,
                Message = "Chapter updated successfully",
                Data = response
            };
        }
    }
}
