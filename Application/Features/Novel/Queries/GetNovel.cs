using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using MongoDB.Driver;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;

namespace Application.Features.Novel.Queries
{
    public class GetNovel : IRequest<ApiResponse>
    {
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
        public string? SearchTerm { get; set; } = "";
        public List<string>? SearchTagTerm { get; set; } = new();
    }

    public class GetNovelHandler : IRequestHandler<GetNovel, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IOpenAIRepository _openAIRepository;
        private readonly ICurrentUserService _currentUserService;
        public GetNovelHandler(INovelRepository novelRepository, IMapper mapper, ITagRepository tagRepository
            , IUserRepository userRepository, IOpenAIRepository openAIRepository, IOpenAIService openAIService
            , ICurrentUserService currentUserService)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
            _openAIRepository = openAIRepository;
            _openAIService = openAIService;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse> Handle(GetNovel request, CancellationToken cancellationToken)
        {
            var result = SystemHelper.ParseSearchQuerySmart(request.SearchTerm);
            var exact = result.Exact;
            var fuzzyTerms = result.FuzzyTerms;
            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);
            var isAdmin = _currentUserService.IsAdmin();
            var currentUserId = _currentUserService.UserId;

            // Exact match trước
            var findCriteria = new FindCreterias
            {
                Page = request.Page,
                Limit = request.Limit,
                SearchTerm = string.IsNullOrWhiteSpace(exact) ? new() : new List<string> { exact },
                SearchTagTerm = request.SearchTagTerm
            };

            var (novels, totalCount) = await _novelRepository.GetAllNovelAsync(
                findCriteria,
                sortBy,
                isAdmin,
                currentUserId
            );

            // Nếu không có thì fallback sang fuzzy search
            if ((novels == null || novels.Count == 0) && fuzzyTerms.Any())
            {
                findCriteria.SearchTerm = fuzzyTerms;
                (novels, totalCount) = await _novelRepository.GetAllNovelAsync(
                    findCriteria,
                    sortBy,
                    isAdmin,
                    currentUserId
                );
            }

            if (novels == null || novels.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No novels found."
                };
            }


            var novelResponse = _mapper.Map<List<NovelResponse>>(novels);
            // Lấy thông tin tác giả và tag
            var authorIds = novels.Select(n => n.author_id).Distinct().ToList();
            var authors = await _userRepository.GetUsersByIdsAsync(authorIds);

            var allTagIds = novels.SelectMany(n => n.tags).Distinct().ToList();
            var allTags = await _tagRepository.GetTagsByIdsAsync(allTagIds);
            var tagDict = allTags.ToDictionary(t => t.id, t => t.name);

            //Xử lý embedding nếu cần
            var novelIds = novels.Select(n => n.id).ToList();
            var existingEmbeddingIds = await _openAIRepository.GetExistingNovelEmbeddingIdsAsync(novelIds);

            var novelsToEmbed = novels.Where(n => !existingEmbeddingIds.Contains(n.id)).ToList();
            Console.WriteLine($"⚠️ Missing embeddings for: {string.Join(",", novelsToEmbed.Select(n => n.id))}");

            var embeddingIds = new List<string>();
            var embeddingInputs = new List<string>();

            if (novelsToEmbed.Any())
            {
                foreach (var novel in novelsToEmbed)
                {
                    // Lấy tag names hợp lệ nếu có
                    var tagNames = novel.tags?
                        .Where(tagId => tagDict.ContainsKey(tagId))
                        .Select(tagId => tagDict[tagId])
                        .ToList() ?? new List<string>();

                    string inputText;

                    if (tagNames.Any())
                    {
                        // Dùng tag names + title (nếu có) giống CreateNovelHandler
                        var titlePart = string.IsNullOrWhiteSpace(novel.title) ? "" : novel.title;
                        inputText = string.Join(", ", tagNames);
                        if (!string.IsNullOrWhiteSpace(titlePart))
                            inputText = $"{inputText} | {titlePart}";
                    }
                    else
                    {
                        // Dùng title + short description
                        var titlePart = string.IsNullOrWhiteSpace(novel.title) ? "" : novel.title;
                        var descPart = string.IsNullOrWhiteSpace(novel.description) ? "" : novel.description;
                        if (!string.IsNullOrWhiteSpace(descPart) && descPart.Length > 300)
                            descPart = descPart.Substring(0, 300);

                        var parts = new List<string>();
                        if (!string.IsNullOrWhiteSpace(titlePart)) parts.Add(titlePart);
                        if (!string.IsNullOrWhiteSpace(descPart)) parts.Add(descPart);

                        inputText = parts.Any() ? string.Join(" | ", parts) : $"novel:{novel.id}";
                    }

                    embeddingIds.Add(novel.id);
                    embeddingInputs.Add(inputText);
                }

                // Gọi API embedding 1 lần cho tất cả inputs
                var vectors = await _openAIService.GetEmbeddingAsync(embeddingInputs);

                // Bảo vệ: kiểm tra số lượng vector trả về khớp số ids
                if (vectors == null || vectors.Count != embeddingIds.Count)
                {
                    Console.WriteLine("⚠️ Embedding count mismatch or null. Skipping saving embeddings to avoid inconsistency.");
                }
                else
                {
                    // Lưu từng vector bằng SaveNovelEmbeddingAsync (giống CreateNovelHandler)
                    for (int idx = 0; idx < embeddingIds.Count; idx++)
                    {
                        var id = embeddingIds[idx];
                        var vec = vectors[idx];
                        try
                        {
                            if (vec != null)
                                await _openAIRepository.SaveNovelEmbeddingAsync(id, vec);
                        }
                        catch (Exception ex)
                        {
                            // Log và tiếp tục với các item còn lại
                            Console.WriteLine($"Error saving embedding for novel {id}: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"✅ Newly embedded novels: {string.Join(',', embeddingIds)}");
                }
            }

            for (int i = 0; i < novels.Count; i++)
            {
                var author = authors.FirstOrDefault(a => a.id == novels[i].author_id);
                if (author != null)
                {
                    novelResponse[i].AuthorName = author.displayname;
                }

                var tags = novels[i].tags;
                novelResponse[i].Tags = allTags
                    .Where(t => tags.Contains(t.id))
                    .Select(t => new TagListResponse
                    {
                        TagId = t.id,
                        Name = t.name
                    }).ToList();
            }

            int totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved novels successfully.",
                Data = new
                {
                    Novels = novelResponse,
                    TotalNovels = totalCount,
                    TotalPages = totalPages,
                    TotalInPage = novels.Count,
                    EmbeddingStats = new
                    {
                        TotalRequested = novelIds.Count,
                        AlreadyExist = existingEmbeddingIds.Count,
                        NewlyEmbedded = embeddingIds.Count,
                        NewlyEmbeddedIds = embeddingIds
                    }
                }
            };
        }

    }
}