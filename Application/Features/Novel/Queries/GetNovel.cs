using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using MongoDB.Driver;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public GetNovelHandler(INovelRepository novelRepository, IMapper mapper, ITagRepository tagRepository, IUserRepository userRepository)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
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
                    TotalPages = totalPages
                }
            };
        }
    }
}
