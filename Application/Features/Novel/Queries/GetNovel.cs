using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Features.Novel.Queries
{
    public class GetNovel: IRequest<ApiResponse>
    {
        public FindCreterias FindCreterias { get; set; }
        public List<SortCreterias>? SortCreterias { get; set; }
    }

    public class GetNovelHandle : IRequestHandler<GetNovel, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;
        public GetNovelHandle(INovelRepository novelRepository, IMapper mapper)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(GetNovel request, CancellationToken cancellationToken)
        {
            var find = request.FindCreterias ?? new FindCreterias { Page = 0, Limit = int.MaxValue, SearchTerm = new List<string>() };
            var sort = request.SortCreterias ?? new List<SortCreterias>();

            var novel = await _novelRepository.GetAllNovelAsync(find, sort);
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
