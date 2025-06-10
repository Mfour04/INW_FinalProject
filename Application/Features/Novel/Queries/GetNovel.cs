using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
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
        public GetNovelHandler(INovelRepository novelRepository, IMapper mapper)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
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

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved novels successfully.",
                Data = novelResponse
            };
        }
    }
}
