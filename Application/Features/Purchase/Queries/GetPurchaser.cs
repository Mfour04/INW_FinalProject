using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Purchase.Queries
{
    public class GetPurchaser: IRequest<ApiResponse>
    {
        public int Page = 0;
        public int Limit = int.MaxValue;
    }
    public class GetPurchaserHandler : IRequestHandler<GetPurchaser, ApiResponse>
    {
        private readonly IOwnershipRepository _ownershipRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IMapper _mapper;
        public GetPurchaserHandler(IOwnershipRepository ownershipRepository, IMapper mapper)
        {
            _ownershipRepository = ownershipRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(GetPurchaser request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new();
            findCreterias.Limit = request.Limit;
            findCreterias.Page = request.Page;

            var purchaser = await _ownershipRepository.GetAllOwnerShipAsync(findCreterias);
            if (purchaser == null || purchaser.Count == 0)
                return new ApiResponse { Success = false, Message = "Purchaser not found" };
            var purchaserRespone = _mapper.Map<List<TagResponse>>(purchaser);
            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved tags successfully.",
                Data = purchaserRespone
            };
        }
    }
}
