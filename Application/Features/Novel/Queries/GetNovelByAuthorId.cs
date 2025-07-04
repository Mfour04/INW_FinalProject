using AutoMapper;
using DnsClient;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Novel.Queries
{
    public class GetNovelByAuthorId: IRequest<ApiResponse>
    {
        public string AuthorId { get; set; }
    }

    public class GetNovelByAuthorIdHandler : IRequestHandler<GetNovelByAuthorId, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;
        public GetNovelByAuthorIdHandler(INovelRepository novelRepository, IMapper mapper)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(GetNovelByAuthorId request, CancellationToken cancellationToken)
        {
            var novelAuthor = await _novelRepository.GetNovelByAuthorId(request.AuthorId);
            if(novelAuthor == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "AuthorId not found"
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Get Novel By Novel By AuthorId Successfully",
                Data = novelAuthor
            };
        }
    }
}
