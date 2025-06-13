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

            if (!string.IsNullOrEmpty(request.SearchTerm))
                findCreterias.SearchTerm = SystemHelper.ParseSearchQuery(request.SearchTerm);

            findCreterias.Limit = request.Limit;

            findCreterias.Page = request.Page;

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var novel = await _novelRepository.GetAllNovelAsync(findCreterias, sortBy);
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
