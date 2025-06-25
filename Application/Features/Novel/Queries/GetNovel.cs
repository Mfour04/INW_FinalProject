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
    public class GetNovel: IRequest<ApiResponse>
    {
        public string SortBy = "created_at:desc";
        public int Page = 0;
        public int Limit = int.MaxValue;
        public string? SearchTerm = "";
    }

    public class GetNovelHandler : IRequestHandler<GetNovel, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;
        public GetNovelHandler(INovelRepository novelRepository, IMapper mapper, ITagRepository tagRepository)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
        }
        public async Task<ApiResponse> Handle(GetNovel request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new();
            var result = SystemHelper.ParseSearchQuerySmart(request.SearchTerm);
            var exact = result.Exact;
            var fuzzyTerms = result.FuzzyTerms;

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            // ----- THỬ EXACT MATCH -----
            var findExact = new FindCreterias
            {
                Page = request.Page,
                Limit = request.Limit,
                SearchTerm = string.IsNullOrEmpty(exact) ? new() : new List<string> { exact }
            };

            var novel = await _novelRepository.GetAllNovelAsync(findExact, sortBy);

            // ----- NẾU KHÔNG CÓ KẾT QUẢ, THỬ FUZZY MATCH -----
            if ((novel == null || novel.Count == 0) && fuzzyTerms.Count > 0)
            {
                var findFuzzy = new FindCreterias
                {
                    Page = request.Page,
                    Limit = request.Limit,
                    SearchTerm = fuzzyTerms
                };

                novel = await _novelRepository.GetAllNovelAsync(findFuzzy, sortBy);
            }

            if (novel == null || novel.Count == 0)
                return new ApiResponse { Success = false, Message = "Novel not found" };
            var novelResponse = _mapper.Map<List<NovelResponse>>(novel);
            var allTagIds = novel.SelectMany(n => n.tags).Distinct().ToList();
            var allTags = await _tagRepository.GetTagsByIdsAsync(allTagIds);

            for (int i = 0; i < novel.Count; i++)
            {
                var tags = novel[i].tags;
                novelResponse[i].Tags = allTags
                    .Where(t => tags.Contains(t.id))
                    .Select(t => new TagListResponse
                    {
                        TagId = t.id,
                        Name = t.name
                    }).ToList();
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved novels successfully.",
                Data = novelResponse
            };
        }
    }
}
