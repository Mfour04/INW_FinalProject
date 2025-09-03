using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;

namespace Application.Features.Novel.Queries
{
    public class GetNovelById : IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
        public string SortBy { get; set; } = "chapter_number:asc";
        public int? ChapterNumber { get; set; }
    }

    public class GetNovelByIdHandler : IRequestHandler<GetNovelById, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IPurchaserRepository _purchaserRepository;
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;

        public GetNovelByIdHandler(
            INovelRepository novelRepository,
            IChapterRepository chapterRepository,
            IPurchaserRepository purchaserRepository,
            IMapper mapper,
            ITagRepository tagRepository,
            IUserRepository userRepository,
            ICurrentUserService currentUser)
        {
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _purchaserRepository = purchaserRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetNovelById request, CancellationToken cancellationToken)
        {
            try
            {
                var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
                if (novel == null)
                    return Fail("Truyện không tồn tại.");

                var novelResponse = _mapper.Map<NovelResponse>(novel);

                var author = (await _userRepository.GetUsersByIdsAsync(new List<string> { novel.author_id }))
               .FirstOrDefault(u => u.id == novel.author_id);
                novelResponse.AuthorName = author?.displayname;

                // Lấy danh sách tagId
                var allTags = await _tagRepository.GetTagsByIdsAsync(novel.tags.Distinct().ToList());
                novelResponse.Tags = allTags
                    .Where(t => novel.tags.Contains(t.id))
                    .Select(t => new TagListResponse { TagId = t.id, Name = t.name })
                    .ToList();

                // Kiểm tra quyền
                var chapterIds = await _chapterRepository.GetIdsByNovelIdAsync(request.NovelId);

                var currentUserId = _currentUser.UserId;
                bool isGuest = string.IsNullOrEmpty(currentUserId);
                bool isAdmin = _currentUser.IsAdmin();
                bool isAuthor = !isGuest && novel.author_id == currentUserId;

                bool hasPurchasedFull = false;
                bool hasPurchasedChapters = false;

                if (!isGuest)
                {
                    hasPurchasedFull = await _purchaserRepository.HasPurchasedFullAsync(currentUserId, request.NovelId);
                    hasPurchasedChapters = await _purchaserRepository.HasAnyPurchasedChapterAsync(currentUserId, request.NovelId, chapterIds);
                }

                if (!novel.is_public && !isAuthor && !isAdmin && !hasPurchasedFull && !hasPurchasedChapters)
                    return Fail("Truyện này chưa được công khai.");

                // Lấy purchased chapters trước để filter
                var purchasedChapterIds = hasPurchasedFull
                    ? new List<string>()
                    : await _purchaserRepository.GetPurchasedChaptersAsync(_currentUser.UserId, request.NovelId);

                // Lấy tất cả chapters có thể truy cập được
                var allChaptersExpanded = await _chapterRepository.GetPagedByNovelIdAsync(
                    request.NovelId,
                    new ChapterFindCreterias
                    {
                        Page = 0,
                        Limit = int.MaxValue,
                        ChapterNumber = request.ChapterNumber
                    },
                    SystemHelper.ParseSortCriteria(request.SortBy)
                );

                // Filter chapters theo quyền truy cập
                var accessibleChapters = allChaptersExpanded.Item1
                    .Where(c =>
                        !c.is_lock && // Ẩn các chapter bị lock
                        (
                            c.is_public ||
                            isAuthor ||
                            isAdmin ||
                            hasPurchasedFull ||
                            purchasedChapterIds.Contains(c.id)
                        )
                    )
                    .ToList();

                // Tìm chapter được cập nhật gần nhất từ tất cả accessible chapters
                var latestUpdatedChapter = accessibleChapters
                    .Where(c => c.created_at != null)
                    .OrderByDescending(c => c.created_at)
                    .FirstOrDefault();

                ChapterResponse latestChapterResponse = null;
                if (latestUpdatedChapter != null)
                {
                    latestChapterResponse = _mapper.Map<ChapterResponse>(latestUpdatedChapter);
                }

                // Phân trang trên kết quả đã filter
                var totalFilteredChapters = accessibleChapters.Count;
                var totalFilteredPages = (int)Math.Ceiling((double)totalFilteredChapters / request.Limit);

                var pagedChapters = accessibleChapters
                    .Skip(request.Page * request.Limit)
                    .Take(request.Limit)
                    .ToList();

                var chapterResponse = _mapper.Map<List<ChapterResponse>>(pagedChapters);

                // Lọc các chương miễn phí từ tất cả chapters (không chỉ từ paged)
                var freeChapterIds = allChaptersExpanded.Item1
                    .Where(c => !c.is_paid && !c.is_lock)
                    .Select(c => c.id)
                    .ToList();

                bool isAccessFull = isAuthor || isAdmin || hasPurchasedFull || (!novel.is_paid && novel.is_public);

                string message = isAccessFull
                    ? "Bạn có thể truy cập toàn bộ truyện."
                    : "Bạn chỉ xem được chương miễn phí và chương đã mua.";

                return new ApiResponse
                {
                    Success = true,
                    Message = message,
                    Data = new
                    {
                        NovelInfo = novelResponse,
                        AllChapters = chapterResponse,
                        LatestUpdatedChapter = latestChapterResponse, // Chapter được cập nhật gần nhất
                        IsAccessFull = isAccessFull,
                        FreeChapters = freeChapterIds,
                        PurchasedChapterIds = isAccessFull ? null : purchasedChapterIds,
                        TotalChapters = totalFilteredChapters, // Số chapter sau khi filter
                        ChapterInCurrentPage = pagedChapters.Count,
                        TotalPages = totalFilteredPages // Số page sau khi filter
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        private ApiResponse Fail(string message) => new ApiResponse { Success = false, Message = message };
    }
}