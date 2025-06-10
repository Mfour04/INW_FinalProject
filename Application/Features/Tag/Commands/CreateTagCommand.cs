using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Features.Tag.Command
{
    public class CreateTagCommand: IRequest<ApiResponse>
    {
        [JsonPropertyName("tag")]
        public CreateTagResponse Tag { get; set; }
    }

    public class CreateTagHandler : IRequestHandler<CreateTagCommand, ApiResponse>
    {
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;

        public CreateTagHandler(ITagRepository tagRepository, IMapper mapper)
        {
            _tagRepository = tagRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            var tag = new TagEntity
            {
                id = SystemHelper.RandomId(),
                name = request.Tag.Name 
            };

            await _tagRepository.CreateTagAsync(tag);
            var tagRepose = _mapper.Map<TagResponse>(tag);

            return new ApiResponse
            {
                Success = true,
                Message = "Created Tag Successfully",
                Data = tagRepose
            };
        }
    }
}
