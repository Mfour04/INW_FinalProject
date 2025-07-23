using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;
using Application.Services.Interfaces;

namespace Application.Features.Novel.Commands
{
    public class UpdateNovelCommand : IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public IFormFile? NovelImage { get; set; }
        public IFormFile? NovelBanner { get; set; }
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
        private readonly ICurrentUserService _currentUserService;
        private readonly ITagRepository _tagRepository;

        public UpdateNovelHandle(INovelRepository novelRepository, IMapper mapper, ICloudDinaryService cloudDinaryService, ICurrentUserService currentUserService, ITagRepository tagRepository)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
            _cloudDinaryService = cloudDinaryService;
            _currentUserService = currentUserService;
            _tagRepository = tagRepository;
        }

        public async Task<ApiResponse> Handle(UpdateNovelCommand request, CancellationToken cancellationToken)
        {
            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Novel not found" };

            var slugExists = await _novelRepository.IsSlugExistsAsync(request.Slug);
            if (slugExists)
                return new ApiResponse { Success = false, Message = "Slug already exists." };

            if (novel.author_id != _currentUserService.UserId)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Unauthorized: You are not the author of this novel"
                };
            }

            novel.title = request.Title ?? novel.title;
            novel.slug = request.Slug ?? novel.slug;
            novel.description = request.Description ?? novel.description;
            if (request.NovelImage != null)
            {
                var novelImageUpdate = await _cloudDinaryService.UploadImagesAsync(request.NovelImage);
                novel.novel_image = novelImageUpdate;
            }
            if (request.NovelBanner != null)
            {
                novel.novel_banner = await _cloudDinaryService.UploadImagesAsync(request.NovelBanner);
            }
            novel.status = request.Status ?? novel.status;
            novel.is_public = request.IsPublic ?? novel.is_public;
            novel.is_lock = request.IsLock ?? novel.is_lock;
            novel.is_paid = request.IsPaid ?? novel.is_paid;
            novel.price = request.Price ?? novel.price;
            if (request.Tags != null && request.Tags.Any())
            {
                novel.tags = request.Tags;
            }
            novel.updated_at = TimeHelper.NowTicks;

            await _novelRepository.UpdateNovelAsync(novel);
            List<TagEntity> tagEntities = new();
            if (novel.tags != null && novel.tags.Any())
            {
                tagEntities = await _tagRepository.GetTagsByIdsAsync(novel.tags);
            }
            var response = _mapper.Map<UpdateNovelResponse>(novel);
            response.Tags = tagEntities.Select(tag => new TagListResponse
            {
                TagId = tag.id,
                Name = tag.name
            }).ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Novel Updated Successfullly",
                Data = response,
            };
        }
    }
}
