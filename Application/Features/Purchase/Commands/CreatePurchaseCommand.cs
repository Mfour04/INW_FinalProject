using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Ownership;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.OwnerShip.Commands
{
    public class CreatePurchaseCommand : IRequest<ApiResponse>
    {
        public CreatePurchaserResponse Purchase { get; set; }
    }

    public class CreatePurchaseHandler : IRequestHandler<CreatePurchaseCommand, ApiResponse>
    {
        private readonly IOwnershipRepository _ownershipRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IMapper _mapper;

        public CreatePurchaseHandler(IOwnershipRepository ownershipRepository, IMapper mapper, IUserRepository userRepository, IChapterRepository chapterRepository, INovelRepository novelRepository)
        {
            _ownershipRepository = ownershipRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _chapterRepository = chapterRepository;
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
        {
            var purchaser = await _userRepository.GetById(request.Purchase.UserId);
            if (purchaser == null)
            {
                return new ApiResponse { Success = false, Message = "Không tìm thấy người mua này" };
            }

            var novel = await _novelRepository.GetByNovelIdAsync(request.Purchase.NovelId);
            if (novel == null)
            {
                return new ApiResponse { Success = false, Message = "Không tìm thấy tiểu thuyết này" };
            }

            var chapter = await _chapterRepository.GetChapterByChapterIdAsync(request.Purchase.ChapterId);
            if (chapter == null)
            {
                return new ApiResponse { Success = false, Message = "Không tìm thấy chương này" };
            }

            var purchase = new PurchaserEntity
            {
                id = SystemHelper.RandomId(),
                user_id = purchaser.id,
                novel_id = novel.id,
                chapter_id = chapter.Select(c => c.id).ToList(),
                is_full = false
            };

            await _ownershipRepository.CreateOwnerShipAsync(purchase);
            var response = _mapper.Map<PurchaserResponse>(purchase);

            return new ApiResponse
            {
                Success = true,
                Message = "Created Purchaser Successfully",
                Data = response
            };
        }
    }
}
