using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Tag.Queries
{
    public class GetTag: IRequest<ApiResponse>
    {
    }
    public class GetTagHandler : IRequestHandler<GetTag, ApiResponse>
    {
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;

        public GetTagHandler(IMapper mapper, ITagRepository tagRepository)
        {
            _mapper = mapper;
            _tagRepository = tagRepository;
        }
        public async Task<ApiResponse> Handle(GetTag request, CancellationToken cancellationToken)
        {
            var tag = await _tagRepository.GetAllTagAsync();
            if(tag == null || tag.Count == 0)
                return new ApiResponse { Success = false, Message = "Tag not found" };
            var tagRespone = _mapper.Map<List<TagResponse>>(tag);
            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved tags successfully.",
                Data = tagRespone
            };
        }
    }
}
