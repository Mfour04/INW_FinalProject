using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Helpers;

namespace Application.Features.Chapter.Queries
{
    public class GetChapterById : IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
        public string? IpAddress { get; set; }
    }

    public class GetChapterByIdHandler : IRequestHandler<GetChapterById, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IPurchaserRepository _purchaserRepository;
        private readonly INovelRepository _novelRepository;
        private readonly INovelViewTrackingRepository _viewTrackingRepository;
        private readonly IChapterHelperService _chapterHelperService;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public GetChapterByIdHandler(IChapterRepository chapterRepository, IPurchaserRepository purchaserRepository,
            INovelRepository novelRepository, INovelViewTrackingRepository viewTrackingRepository,
            IChapterHelperService chapterHelperService, IMapper mapper, ICurrentUserService currentUser)
        {
            _chapterRepository = chapterRepository;
            _purchaserRepository = purchaserRepository;
            _novelRepository = novelRepository;
            _viewTrackingRepository = viewTrackingRepository;
            _chapterHelperService = chapterHelperService;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetChapterById request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
                return new ApiResponse { Success = false, Message = "Chapter not found" };

            var novel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Novel not found" };

            var previousChapter = await _chapterRepository.GetPreviousAsync(novel.id, chapter.chapter_number ?? 0);
            var nextChapter = await _chapterRepository.GetNextAsync(novel.id, chapter.chapter_number ?? 0);

            var viewerId = !string.IsNullOrEmpty(userId) ? userId : request.IpAddress;

            bool isAdmin = _currentUser.IsAdmin();
            bool isAuthor = userId == novel.author_id;
            bool hasFullOwnerShip = await _purchaserRepository.HasPurchasedFullAsync(userId, novel.id);
            bool hasChapterOwnership = await _purchaserRepository.HasPurchasedChapterAsync(userId, novel.id, chapter.id);

            bool hasAccess = isAdmin || isAuthor || hasFullOwnerShip || hasChapterOwnership;

            if (!novel.is_public || !chapter.is_public)
            {
                if (!hasAccess)
                    return new ApiResponse { Success = false, Message = "Nội dung này đã bị ẩn." };
            }

            bool shouldReloadChapter = false;

            if (!chapter.is_paid)
            {
                await HandleNovelViewAsync(userId, novel.id);
                await _chapterHelperService.ProcessViewAsync(chapter.id, viewerId);
                shouldReloadChapter = true;
            }
            else
            {
                if (string.IsNullOrEmpty(userId) && !isAdmin)
                    return new ApiResponse { Success = false, Message = "Bạn chưa đăng nhập để xem chương này." };

                if (hasAccess)
                {
                    await HandleNovelViewAsync(userId, novel.id);
                    await _chapterHelperService.ProcessViewAsync(chapter.id, viewerId);
                    shouldReloadChapter = true;
                }
                else
                {
                    return new ApiResponse { Success = false, Message = "Bạn chưa mua chương này." };
                }
            }

            if (shouldReloadChapter)
            {
                chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            }

            var chapterResponse = _mapper.Map<ChapterResponse>(chapter);
            return new ApiResponse
            {
                Success = true,
                Data = new
                {
                    Chapter = chapterResponse,
                    PreviousChapterId = previousChapter?.id,
                    NextChapterId = nextChapter?.id
                }
            };

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
                    created_at = TimeHelper.NowTicks,
                    updated_at = TimeHelper.NowTicks
                };
                await _viewTrackingRepository.CreateViewTrackingAsync(newTracking);
                await _novelRepository.IncreaseTotalViewAsync(novelId);
            }
            else
            {
                var lastViewDate = new DateTime(tracking.updated_at).Date;
                if (lastViewDate < today)
                {
                    tracking.updated_at = TimeHelper.NowTicks;
                    await _viewTrackingRepository.UpdateViewTrackingAsync(tracking);
                    await _novelRepository.IncreaseTotalViewAsync(novelId);
                }
            }
        }
    }
}
