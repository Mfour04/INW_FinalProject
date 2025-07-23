using Application.Services.Interfaces;
using AutoMapper;
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
        public GetNovelHandler(INovelRepository novelRepository, IMapper mapper, ITagRepository tagRepository
            , IUserRepository userRepository, IOpenAIRepository openAIRepository, IOpenAIService openAIService)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
            _openAIRepository = openAIRepository;
            _openAIService = openAIService;
        }

        public async Task<ApiResponse> Handle(GetNovel request, CancellationToken cancellationToken)
        {
            var result = SystemHelper.ParseSearchQuerySmart(request.SearchTerm);
            var exact = result.Exact;
            var fuzzyTerms = result.FuzzyTerms;
            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            // Exact match trước
            var findCriteria = new FindCreterias
            {
                Page = request.Page,
                Limit = request.Limit,
                SearchTerm = string.IsNullOrWhiteSpace(exact) ? new() : new List<string> { exact },
                SearchTagTerm = request.SearchTagTerm
            };

            var (novels, totalCount) = await _novelRepository.GetAllNovelAsync(findCriteria, sortBy);
            
            // Fallback: nếu không có, thử fuzzy
            if ((novels == null || novels.Count == 0) && fuzzyTerms.Any())
            {
                findCriteria.SearchTerm = fuzzyTerms;
                (novels, totalCount) = await _novelRepository.GetAllNovelAsync(findCriteria, sortBy);
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
            var authorIds = novels.Select(n => n.author_id).Distinct().ToList();
            var authors = await _userRepository.GetUsersByIdsAsync(authorIds);
            var allTagIds = novels.SelectMany(n => n.tags).Distinct().ToList();
            var allTags = await _tagRepository.GetTagsByIdsAsync(allTagIds);

            //embedding cho tất cả tag của các novel
            var novelIds = novels.Select(n => n.id).ToList();
            var existingEmbeddingIds = await _openAIRepository.GetExistingNovelEmbeddingIdsAsync(novelIds);

            var novelsToEmbed = novels.Where(n => !existingEmbeddingIds.Contains(n.id)).ToList();
            var newlyEmbeddedIds = novelsToEmbed.Select(n => n.id).ToList();

            if (novelsToEmbed.Any())
            {
                var tagNames = novelsToEmbed.Select(n => n.tags)
                    .Select(tags => string.Join(", ", tags))
                    .ToList();

                var vectors = await _openAIService.GetEmbeddingAsync(tagNames);
                await _openAIRepository.SaveListNovelEmbeddingAsync(newlyEmbeddedIds, vectors);
            }


            for (int i = 0; i < novels.Count; i++)
            {
                var author = authors.FirstOrDefault(a => a.id == novels[i].author_id);
                if (author != null)
                {
                    novelResponse[i].AuthorName = author.displayname; // hoặc author.FullName, tùy DB bạn lưu
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
                    EmbeddingStats = new
                    {
                        TotalRequested = novelIds.Count,
                        AlreadyExist = existingEmbeddingIds.Count,
                        NewlyEmbedded = newlyEmbeddedIds.Count,
                        NewlyEmbeddedIds = newlyEmbeddedIds
                    }
                }
            };
        }
    }
}
