using AutoMapper;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;

namespace Application.Features.Novel.Queries
{
    public class GetNovelById : IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public string UserId { get; set; }
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
        public GetNovelByIdHandler(
            INovelRepository novelRepository,
            IChapterRepository chapterRepository,
            IPurchaserRepository purchaserRepository,
            IMapper mapper,
            ITagRepository tagRepository,
            IUserRepository userRepository)
        {
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _purchaserRepository = purchaserRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(GetNovelById request, CancellationToken cancellationToken)
        {
            try
            {
                var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
                if (novel == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Novel not found"
                    };
                }

                var novelResponse = _mapper.Map<NovelResponse>(novel);

                var authors = await _userRepository.GetUsersByIdsAsync(new List<string> { novel.author_id });
                var author = authors.FirstOrDefault(a => a.id == novel.author_id);
                if (author != null)
                {
                    novelResponse.AuthorName = author.displayname; // hoặc FullName nếu bạn dùng
                }


                // ✅ Lấy danh sách tagId
                var allTagIds = novel.tags.Distinct().ToList();
                var allTags = await _tagRepository.GetTagsByIdsAsync(allTagIds);

                // ✅ Map sang TagListResponse
                novelResponse.Tags = allTags
                    .Where(t => novel.tags.Contains(t.id))
                    .Select(t => new TagListResponse
                    {
                        TagId = t.id,
                        Name = t.name
                    }).ToList();

                var chapterCriteria = new ChapterFindCreterias
                {
                    Page = request.Page,
                    Limit = request.Limit,
                    ChapterNumber = request.ChapterNumber
                };

                var sort = SystemHelper.ParseSortCriteria(request.SortBy);

                var (allChapterEntities, totalChapters, totalPages) = await _chapterRepository.GetPagedByNovelIdAsync(request.NovelId, chapterCriteria, sort);
                var allChapterIds = allChapterEntities.Select(c => c.id).ToList();

               // Lọc các chương miễn phí
                var freeChapterIds = allChapterEntities
                    .Where(c => !c.is_paid)
                    .Select(c => c.id)
                    .ToList();

                // Kiểm tra người dùng (guest, tác giả, đã mua truyện, v.v.)
                bool isGuest = string.IsNullOrEmpty(request.UserId);
                bool isAuthor = !isGuest && novel.author_id == request.UserId;
                bool hasPurchasedFull = !isGuest && await _purchaserRepository.HasPurchasedFullAsync(request.UserId, request.NovelId);
                var purchasedChapterIds = hasPurchasedFull ? allChapterIds : await _purchaserRepository.GetPurchasedChaptersAsync(request.UserId, request.NovelId);

                // Trả về lỗi nếu truyện chưa public, và người dùng không phải tác giả hoặc chưa mua full.
                if (!novel.is_public && !isAuthor && !hasPurchasedFull)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Truyện này chưa được công khai."
                    };
                }

                // Trường hợp truyện miễn phí và đã công khai
                if (!novel.is_paid && novel.is_public)
                {
                    return new ApiResponse
                    {
                        Success = true,
                        Data = new
                        {
                            NovelInfo = novelResponse,
                            AllChapters = allChapterEntities,
                            PurchasedChapterIds = purchasedChapterIds,
                            TotalChapters = totalChapters,
                            TotalPages = totalPages,
                            FreeChapters = freeChapterIds
                        }
                    };
                }

                // Nếu người dùng đã mua toàn bộ truyện hoặc là tác giả, trả về tất cả chương
                if (hasPurchasedFull || isAuthor)
                {
                    return new ApiResponse
                    {
                        Success = true,
                        Data = new
                        {
                            NovelInfo = novel,
                            AllChapters = allChapterIds
                        }
                    };
                }

                // Nếu người dùng chưa mua toàn bộ, chỉ trả về các chương đã mua và miễn phí
                if (novel.is_paid)
                {
                    // Nếu truyện đã hoàn thành và người dùng chưa mua full → chỉ xem được chương miễn phí.
                    if (novel.status == NovelStatus.Completed)
                    {
                        if (!isAuthor && !hasPurchasedFull)
                        {
                            return new ApiResponse
                            {
                                Success = true,
                                Data = new
                                {
                                    NovelInfo = novelResponse,
                                    AllChapters = allChapterEntities,
                                    FreeChapters = freeChapterIds,
                                    PurchasedChapterIds = purchasedChapterIds,
                                    TotalChapters = totalChapters,
                                    TotalPages = totalPages,
                                    Message = "Bạn chưa mua truyện này (đã hoàn thành). Chỉ xem được chương miễn phí."
                                }
                            };
                        }
                    }
                    // Nếu truyện đang ra và người dùng chưa mua chương nào → chỉ xem được chương miễn phí.
                    else if (novel.status == NovelStatus.Ongoing)
                    {
                        if (!isAuthor && !purchasedChapterIds.Any())
                        {
                            return new ApiResponse
                            {
                                Success = true,
                                Data = new
                                {
                                    NovelInfo = novelResponse,
                                    AllChapters = allChapterEntities,
                                    FreeChapters = freeChapterIds,
                                    TotalChapters = totalChapters,
                                    TotalPages = totalPages,
                                    PurchasedChapterIds = purchasedChapterIds,
                                }
                            };
                        }
                    }
                }

                // Trường hợp mặc định: người dùng là tác giả hoặc đã mua một phần hoặc toàn bộ truyện → trả về đầy đủ.
                return new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        NovelInfo = novelResponse,
                        AllChapters = allChapterEntities,
                        PurchasedChapterIds = purchasedChapterIds,
                        TotalChapters = totalChapters,
                        TotalPages = totalPages,
                        FreeChapters = freeChapterIds,
                        Message = "Bạn chưa mua toàn bộ truyện, chỉ xem được các chương đã mua và miễn phí."
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
    }
}
