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
            var deleted= await _chapterRepository.DeleteChapterAsync(request.ChapterId);
            return new ApiResponse
            {
                Success = true,
                Message = "Chapter Deleted Succuessfully",
                Data = deleted
            };
        }
    }
}
