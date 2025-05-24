using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Novel.Commands
{
    public class DeleteNovelCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
    }
    public class DeleteNovelHandler : IRequestHandler<DeleteNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;

        public DeleteNovelHandler(INovelRepository novelRepository, IMapper mapper)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(DeleteNovelCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _novelRepository.DeleteNovelAsync(request.NovelId);

            return new ApiResponse
            {
                Success = true,
                Message = "Novel Deleted Succuessfully",
                Data = deleted
            };
        }
    }
}
