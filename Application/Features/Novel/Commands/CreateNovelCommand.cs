using Application.Features.User.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;
using System.Text.Json.Serialization;

namespace Application.Features.Novel.Commands
{
    public class CreateNovelCommand: IRequest<ApiResponse>
    {
        [JsonPropertyName("novel")]
        public CreateNovelResponse Novel { get; set; }
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
            var author = await _userRepository.GetById(request.Novel.AuthorId);
            if (author == null)
            {
                return new ApiResponse { Success = false, Message = "Author not found" };
            }

            var validTagIds = new List<string>();
            if (request.Novel.Tags != null && request.Novel.Tags.Any())
            {
                var existingTags = await _tagRepository.GetTagsByIdsAsync(request.Novel.Tags);
                validTagIds = existingTags.Select(t => t.id).ToList();
            }

            var novelImage = await _cloudDinaryService.UploadImagesAsync(request.Novel.NovelImage);

            var novel = new NovelEntity
            {
                id = SystemHelper.RandomId(),
                title = request.Novel.Title,
                title_unsigned = SystemHelper.RemoveDiacritics(request.Novel.Title),
                description = request.Novel.Description,
                author_id = author.id,
                novel_image = novelImage,
                tags = validTagIds,
                status = request.Novel.Status,
                is_lock = false,
                is_public = request.Novel.IsPublic ?? false,
                is_paid = request.Novel.IsPaid ?? false,
                purchase_type = request.Novel.PurchaseType,
                price = request.Novel.Price ?? 0,
                created_at = DateTime.UtcNow.Ticks,
                updated_at = DateTime.UtcNow.Ticks
            };

            await _novelRepository.CreateNovelAsync(novel);
            var tags = await _tagRepository.GetTagsByIdsAsync(validTagIds);
            var response = _mapper.Map<NovelResponse>(novel);

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
