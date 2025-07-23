using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Application.Services.Interfaces;

namespace Application.Features.Novel.Commands
{
    public class CreateNovelCommand : IRequest<ApiResponse>
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string? AuthorId { get; set; }
        public IFormFile? NovelImage { get; set; }
        public IFormFile? NovelBanner { get; set; }
        public List<string> Tags { get; set; } = new();
        public NovelStatus Status { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsLock { get; set; }
        public bool? AllowComment { get; set; }
        public int? Price { get; set; }
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
                return Fail("Author not found");

            if (string.IsNullOrWhiteSpace(request.Slug))
                return Fail("Slug is required.");

            var slugExists = await _novelRepository.IsSlugExistsAsync(request.Slug);
            if (slugExists)
                return Fail("Slug already exists.");

            var validTagIds = new List<string>();
            if (request.Tags != null && request.Tags.Any())
            {
                var existingTags = await _tagRepository.GetTagsByIdsAsync(request.Tags);
                validTagIds = existingTags.Select(t => t.id).ToList();
            }

            var novelImage = await _cloudDinaryService.UploadImagesAsync(request.NovelImage);
            var novelBanner = await _cloudDinaryService.UploadImagesAsync(request.NovelBanner);

            var novel = new NovelEntity
            {
                id = SystemHelper.RandomId(),
                title = request.Title,
                title_unsigned = SystemHelper.RemoveDiacritics(request.Title),
                slug = request.Slug,
                description = request.Description,
                author_id = author.id,
                novel_image = novelImage,
                novel_banner = novelBanner,
                tags = validTagIds,
                status = request.Status,
                is_lock = false,
                is_public = request.IsPublic ?? false,
                allow_comment = request.AllowComment ?? true,
                is_paid = request.IsPaid ?? false,
                price = request.Price ?? 0,
                total_chapters = 0,
                total_views = 0,
                followers = 0,
                rating_avg = 0,
                rating_count = 0,
                created_at = TimeHelper.NowTicks,
                updated_at = TimeHelper.NowTicks
            };

            await _novelRepository.CreateNovelAsync(novel);

            var tags = await _tagRepository.GetTagsByIdsAsync(validTagIds);
            var response = _mapper.Map<CreateNovelResponse>(novel);

            response.AuthorId = novel.author_id;
            response.AuthorName = author.displayname;
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

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
