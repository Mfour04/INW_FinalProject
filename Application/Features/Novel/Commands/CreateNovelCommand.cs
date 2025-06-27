using Application.Features.User.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Novel.Commands
{
    public class CreateNovelCommand: IRequest<ApiResponse>
    {
        [JsonPropertyName("novel")]
        public string Title { get; set; }
        public string Description { get; set; }
        public string AuthorId { get; set; }
        public IFormFile? NovelImage { get; set; }
        public List<string> Tags { get; set; } = new();
        public NovelStatus Status { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsLock { get; set; }
        public PurchaseType PurchaseType { get; set; }
        public int? Price { get; set; }
        public int TotalChapters { get; set; }
        public int TotalViews { get; set; }
        public int Followers { get; set; }
        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }
    }

    public class CreateNovelHandler : IRequestHandler<CreateNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;
        private readonly ICloudDinaryService _cloudDinaryService;
        public CreateNovelHandler(INovelRepository novelRepository, IMapper mapper, IUserRepository userRepository, ITagRepository tagRepository, ICloudDinaryService cloudDinaryService)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
            _cloudDinaryService = cloudDinaryService;
        }
        public async Task<ApiResponse> Handle(CreateNovelCommand request, CancellationToken cancellationToken)
        {
            var author = await _userRepository.GetById(request.AuthorId);
            if (author == null)
            {
                return new ApiResponse { Success = false, Message = "Author not found" };
            }

            var validTagIds = new List<string>();
            if (request.Tags != null && request.Tags.Any())
            {
                var existingTags = await _tagRepository.GetTagsByIdsAsync(request.Tags);
                validTagIds = existingTags.Select(t => t.id).ToList();
            }

            var novelImage = await _cloudDinaryService.UploadImagesAsync(request.NovelImage);

            var novel = new NovelEntity
            {
                id = SystemHelper.RandomId(),
                title = request.Title,
                title_unsigned = SystemHelper.RemoveDiacritics(request.Title),
                description = request.Description,
                author_id = author.id,
                novel_image = novelImage,
                tags = validTagIds,
                status = request.Status,
                is_lock = false,
                is_public = request.IsPublic ?? false,
                is_paid = request.IsPaid ?? false,
                purchase_type = request.PurchaseType,
                price = request.Price ?? 0,
                created_at = DateTime.UtcNow.Ticks,
                updated_at = DateTime.UtcNow.Ticks
            };

            await _novelRepository.CreateNovelAsync(novel);
            var tags = await _tagRepository.GetTagsByIdsAsync(validTagIds);
            var response = _mapper.Map<CreateNovelResponse>(novel);

            response.Tags = tags.Select(t => new TagListResponse
            {
                TagId = t.id,
                Name = t.name
            }).ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Created Novel Successfully",
                Data = response
            };
        }
    }
}
