using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Novel.Commands
{
    public class UpdateNovelCommand: IRequest<ApiResponse>
    {
        public UpdateNovelResponse UpdateNovel { get; set; }
    }

    public class UpdateNovelHandle : IRequestHandler<UpdateNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;

        public UpdateNovelHandle(INovelRepository novelRepository, IMapper mapper)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(UpdateNovelCommand request, CancellationToken cancellationToken)
        {
            var input = request.UpdateNovel;
            var novel = await _novelRepository.GetByNovelIdAsync(input.NovelId);
            if(novel == null)
                return new ApiResponse { Success = false, Message = "Novel not found" };

            novel.title = input.Title ?? novel.title;
            novel.description = input.Description ?? novel.description;
            novel.status = input.Status ?? novel.status;
            novel.is_public = input.IsPublic ?? novel.is_public;
            novel.is_paid = input.IsPaid ?? novel.is_paid;
            novel.price = input.Price ?? novel.price;
            novel.tags = input.Tags ?? novel.tags;
            novel.purchase_type = input.PurchaseType ?? novel.purchase_type;
            novel.updated_at = DateTime.UtcNow.Ticks;

            await _novelRepository.UpdateNovelAsync(novel);

            var response = _mapper.Map<UpdateNovelResponse>(novel);

            return new ApiResponse
            {
                Success = true,
                Message = "Novel Updated Successfullly",
                Data = response,
            };
        }
    }
}
