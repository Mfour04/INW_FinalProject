using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Contracts.Response.Tag;

namespace Application.Features.Tag.Command
{
    public class UpdateTagCommand : IRequest<ApiResponse>
    {
        public string TagId { get; set; }
        public string Name { get; set; }
    }

    public class UpdateTagHandle : IRequestHandler<UpdateTagCommand, ApiResponse>
    {
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;

        public UpdateTagHandle(ITagRepository tagRepository, IMapper mapper)
        {
            _tagRepository = tagRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
        {
            var tag = await _tagRepository.GetByTagIdAsync(request.TagId);
            if (tag == null)
                return new ApiResponse { Success = false, Message = "Tag not found" };

            tag.name = request.Name ?? tag.name;

            await _tagRepository.UpdateTagAsync(tag);
            var response = _mapper.Map<UpdateTagResponse>(tag);

            return new ApiResponse
            {
                Success = true,
                Message = "Tag Updated Successfullly",
                Data = response,
            };
        }
    }
}
