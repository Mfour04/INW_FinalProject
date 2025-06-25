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
    public class ScheduleChapterReleaseCommand: IRequest<ApiResponse>
    {
    }
    public class ScheduleChapterReleaseHanlder : IRequestHandler<ScheduleChapterReleaseCommand, ApiResponse>
    {
        private IChapterRepository _chapterRepository;
        public ScheduleChapterReleaseHanlder(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }
        public async Task<ApiResponse> Handle(ScheduleChapterReleaseCommand request, CancellationToken cancellationToken)
        {
            var updatedCount = await _chapterRepository.ReleaseScheduledChaptersAsync();

            return new ApiResponse
            {
                Success = true,
                Message = $"{updatedCount} chapter(s) released.",
                Data = updatedCount
            };
        }
    }
}
