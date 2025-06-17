using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using System.Text.Json.Serialization;

namespace Application.Features.Novel.Commands
{
    public class UpdateNovelCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile? NovelImage { get; set; }
        public NovelStatus? Status { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsLock { get; set; }
        public bool? IsPaid { get; set; }
        public int? Price { get; set; }
        public List<string>? Tags { get; set; }
        public PurchaseType? PurchaseType { get; set; }
    }

    public class UpdateNovelHandle : IRequestHandler<UpdateNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;
        private readonly ICloudDinaryService _cloudDinaryService;
        public UpdateNovelHandle(INovelRepository novelRepository, IMapper mapper, ICloudDinaryService cloudDinaryService)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
            _cloudDinaryService = cloudDinaryService;
        }
        public async Task<ApiResponse> Handle(UpdateNovelCommand request, CancellationToken cancellationToken)
        {
            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if(novel == null)
                return new ApiResponse { Success = false, Message = "Novel not found" };

            novel.title = request.Title ?? novel.title;
            novel.description = request.Description ?? novel.description;
            if(request.NovelImage != null)
            {
                var novelImageUpdate = await _cloudDinaryService.UploadImagesAsync(request.NovelImage);
                novel.novel_image = novelImageUpdate;
            }
            novel.status = request.Status ?? novel.status;
            novel.is_public = request.IsPublic ?? novel.is_public;
            novel.is_lock = request.IsLock ?? novel.is_lock;
            novel.is_paid = request.IsPaid ?? novel.is_paid;
            novel.price = request.Price ?? novel.price;
            novel.tags = request.Tags ?? novel.tags;
            novel.purchase_type = request.PurchaseType ?? novel.purchase_type;
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
