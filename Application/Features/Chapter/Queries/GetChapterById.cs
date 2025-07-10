using Domain.Entities;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using MongoDB.Bson;
using Shared.Contracts.Response;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Chapter.Queries
{
    public class GetChapterById: IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
        public string UserId { get; set; }
    }
    public class GetChapterByIdHandler : IRequestHandler<GetChapterById, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IPurchaserRepository _purchaserRepository;
        private readonly INovelRepository _novelRepository;
        private readonly INovelViewTrackingRepository _viewTrackingRepository;
        public GetChapterByIdHandler(IChapterRepository chapterRepository, IPurchaserRepository purchaserRepository, INovelRepository novelRepository, INovelViewTrackingRepository viewTrackingRepository)
        {
            _chapterRepository = chapterRepository;
            _purchaserRepository = purchaserRepository;
            _novelRepository = novelRepository;
            _viewTrackingRepository = viewTrackingRepository;
        }

        public async Task<ApiResponse> Handle(GetChapterById request, CancellationToken cancellationToken)
        {
            var chapter = await _chapterRepository.GetByChapterIdAsync(request.ChapterId);
            if (chapter == null)
                return new ApiResponse { Success = false, Message = "Chapter not found" };

            var novelId = chapter.novel_id;
            var novel = await _novelRepository.GetByNovelIdAsync(novelId);
            if (novel == null)
            {
                return new ApiResponse { Success = false, Message = "Novel not found" };
            }

            var previousChapter = await _chapterRepository.GetPreviousChapterAsync(novelId, chapter.chapter_number ?? 0);
            var nextChapter = await _chapterRepository.GetNextChapterAsync(novelId, chapter.chapter_number ?? 0);

            if (!chapter.is_paid)
            {
                await HandleNovelViewAsync(request.UserId, novelId);
                return new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        Chapter = chapter,
                        PreviousChapterId = previousChapter?.id,
                        NextChapterId = nextChapter?.id
                    }
                };
            }


            if (chapter.is_paid)
            {
                bool hasFullOwnerShip = await _purchaserRepository.HasPurchasedFullAsync(request.UserId, novelId);
                bool hasFullChapter = await _purchaserRepository.HasPurchasedChapterAsync(request.UserId, novelId, request.ChapterId);
                if ( hasFullOwnerShip || hasFullChapter)
                {
                    await HandleNovelViewAsync(request.UserId, novelId);
                    return new ApiResponse
                    {
                        Success = true,
                        Data = new
                        {
                            Chapter = chapter,
                            PreviousChapterId = previousChapter?.id,
                            NextChapterId = nextChapter?.id
                        }
                    };
                }
                else
                {
                    return new ApiResponse { Success = false, Message = "Bạn chưa mua chương này." };
                }

            }
            return new ApiResponse { Success = false, Message = "Bạn không có quyền xem chương này." };
        }

        private async Task HandleNovelViewAsync(string userId, string novelId)
        {
            var tracking = await _viewTrackingRepository.FindByUserAndNovelAsync(userId, novelId);
            var today = DateTime.UtcNow.Date;
            if (tracking == null)
            {
                var newTracking = new NovelViewTrackingEntity
                {
                    id = SystemHelper.RandomId(),
                    user_id = userId,
                    novel_id = novelId,
                    created_at = DateTime.UtcNow.Ticks,
                    updated_at = DateTime.UtcNow.Ticks
                };
                await _viewTrackingRepository.CreateViewTrackingAsync(newTracking);
                await _novelRepository.IncreaseTotalViewAsync(novelId);
            }
            else
            {
                var lastViewDate = new DateTime(tracking.updated_at).Date;
                if (lastViewDate < today)
                {
                    tracking.updated_at = DateTime.UtcNow.Ticks;
                    await _viewTrackingRepository.UpdateViewTrackingAsync(tracking);
                    await _novelRepository.IncreaseTotalViewAsync(novelId);
                }
            }
        }
    }
}
