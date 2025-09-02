using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;

namespace Application.Features.Novel.Queries
{
    public class GetNovelBySlug : IRequest<ApiResponse>
    {
        public string? SlugName { get; set; }
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
        public string SortBy { get; set; } = "chapter_number:asc";
        public int? ChapterNumber { get; set; }
    }

    public class GetNovelBySlugHandler : IRequestHandler<GetNovelBySlug, ApiResponse>
    {
        private readonly INovelRepository _novelRepo;
        private readonly IChapterRepository _chapterRepo;
        private readonly IPurchaserRepository _purchaserRepo;
        private readonly ITagRepository _tagRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public GetNovelBySlugHandler(
           INovelRepository novelRepo,
           IChapterRepository chapterRepo,
           IPurchaserRepository purchaserRepo,
           ITagRepository tagRepo,
           IUserRepository userRepo,
           IMapper mapper,
           ICurrentUserService currentUser)
        {
            _novelRepo = novelRepo;
            _chapterRepo = chapterRepo;
            _purchaserRepo = purchaserRepo;
            _tagRepo = tagRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetNovelBySlug request, CancellationToken cancellationToken)
        {
            var novel = await _novelRepo.GetBySlugAsync(request.SlugName);
            if (novel == null)
                return Fail("Truyện không tồn tại.");

            var novelResponse = _mapper.Map<NovelResponse>(novel);

            var author = (await _userRepo.GetUsersByIdsAsync(new List<string> { novel.author_id }))
             .FirstOrDefault(u => u.id == novel.author_id);
            // hoặc FullName nếu bạn dùng
            novelResponse.AuthorName = author?.displayname;

            // Lấy danh sách tagId
            var allTags = await _tagRepo.GetTagsByIdsAsync(novel.tags.Distinct().ToList());
            novelResponse.Tags = allTags
                .Where(t => novel.tags.Contains(t.id))
                .Select(t => new TagListResponse { TagId = t.id, Name = t.name })
                .ToList();

            // Kiểm tra quyền
            bool isGuest = string.IsNullOrEmpty(_currentUser.UserId);
            bool isAdmin = _currentUser.IsAdmin();
            bool isAuthor = !isGuest && novel.author_id == _currentUser.UserId;
            bool hasPurchasedFull = !isGuest && await _purchaserRepo.HasPurchasedFullAsync(_currentUser.UserId, novel.id);

            if (!novel.is_public && !isAuthor && !hasPurchasedFull)
                return Fail("Truyện này chưa được công khai.");

            var chapterCriteria = new ChapterFindCreterias
            {
                Page = request.Page,
                Limit = request.Limit,
                ChapterNumber = request.ChapterNumber
            };
            var sort = SystemHelper.ParseSortCriteria(request.SortBy);

            var (allChapterEntities, totalChapters, totalPages) = await _chapterRepo.GetPagedByNovelIdAsync(novel.id, chapterCriteria, sort);

            if (!isAuthor && !isAdmin)
            {
                allChapterEntities = allChapterEntities
                    .Where(c => !c.is_lock)
                    .ToList();
            }

            var allChapterIds = allChapterEntities.Select(c => c.id).ToList();
            var chapterResponse = _mapper.Map<List<ChapterResponse>>(allChapterEntities);

            // Lọc các chương miễn phí
            var freeChapterIds = allChapterEntities
                .Where(c => !c.is_paid)
                .Select(c => c.id)
                .ToList();

            // Chương đã mua (nếu chưa mua full)
            var purchasedChapterIds = hasPurchasedFull
                ? new List<string>()
                : await _purchaserRepo.GetPurchasedChaptersAsync(_currentUser.UserId, novel.id);

            bool isAccessFull = isAdmin || isAuthor || hasPurchasedFull || (!novel.is_paid && novel.is_public);

            string message = isAccessFull
                ? "Bạn có thể truy cập toàn bộ truyện."
                : "Bạn chỉ xem được chương miễn phí và chương đã mua.";

            return new ApiResponse
            {
                Success = true,
                Data = new
                {
                    NovelInfo = novelResponse,
                    AllChapters = chapterResponse,
                    IsAccessFull = isAccessFull,
                    FreeChapters = freeChapterIds,
                    PurchasedChapterIds = isAccessFull ? null : purchasedChapterIds,
                    TotalChapters = totalChapters,
                    TotalPages = totalPages,
                    Message = message
                }
            };
        }

        private ApiResponse Fail(string message) => new ApiResponse { Success = false, Message = message };
    }
}
