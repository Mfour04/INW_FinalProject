using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Contracts.Response.User;
using Shared.Helpers;
using System.ComponentModel.DataAnnotations;
using static Domain.Entities.UserEntity;

namespace Application.Features.User.Feature
{
    public class UpdateUserProfileCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        [Required(ErrorMessage = "DisplayName is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "DisplayName must be between 2 and 100 characters")]
        public string DisplayName { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        public string Bio { get; set; }

        public IFormFile? AvatarUrl { get; set; }
        public IFormFile? CoverUrl { get; set; }
        public List<string> BadgeId { get; set; } = new();
        public List<string>? FavouriteType { get; set; } = new();
    }
    public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICloudDinaryService _cloudDinaryService;
        private readonly IOpenAIService _openAIService;
        private readonly IOpenAIRepository _openAIRepository;
        private readonly ITagRepository _tagRepository;
        public UpdateUserProfileHandler(IUserRepository userRepository, IMapper mapper
            , ICloudDinaryService cloudDinaryService, IOpenAIService openAIService
            , IOpenAIRepository openAIRepository, ITagRepository tagRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _cloudDinaryService = cloudDinaryService;
            _openAIService = openAIService;
            _openAIRepository = openAIRepository;
            _tagRepository = tagRepository;
        }
        public async Task<ApiResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserId);
            if(user == null)
            {
                return new ApiResponse { Success = false, Message = "User not found." };
            }
            var oldFavouriteTypes = user.favourite_type ?? new List<string>();
            user.displayname = request.DisplayName;
            user.displayname_unsigned = SystemHelper.RemoveDiacritics(request.DisplayName);
            user.displayname_normalized = SystemHelper.RemoveDiacritics(request.DisplayName);
            user.bio = request.Bio;
            user.updated_at = DateTime.UtcNow.Ticks;
            user.badge_id = request.BadgeId;
            user.favourite_type = request.FavouriteType;

            if (request.AvatarUrl != null)
            {
                var imageAUrl = await _cloudDinaryService.UploadImagesAsync(request.AvatarUrl, CloudFolders.Users);
                user.avata_url = imageAUrl; // ✅ Sử dụng đúng field name từ database: avata_url
            }

            if (request.CoverUrl != null)
            {
                var coverUrl = await _cloudDinaryService.UploadImagesAsync(request.CoverUrl, CloudFolders.Users);
                user.cover_url = coverUrl;
                Console.WriteLine($"✅ Cover image uploaded successfully: {coverUrl}");
            }
            else
            {
                Console.WriteLine($"ℹ️ No cover image provided for user {request.UserId}");
            }

            if (user.favourite_type != null && user.favourite_type.Any())
            {
                var hasChanged = !oldFavouriteTypes.OrderBy(x => x).SequenceEqual(user.favourite_type.OrderBy(x => x));

                if (hasChanged)
                {
                    var favouriteTags = await _tagRepository.GetTagsByIdsAsync(user.favourite_type);
                    var tagNames = favouriteTags.Select(tag => tag.name).ToList();

                    if (tagNames.Any())
                    {
                        var embeddingInput = string.Join(", ", tagNames);
                        var vectors = await _openAIService.GetEmbeddingAsync(new List<string> { embeddingInput });

                        var vector = vectors.FirstOrDefault();
                        if (vector != null)
                        {
                            await _openAIRepository.SaveUserEmbeddingAsync(user.id, vector);
                        }
                    }
                }
            }

            var updateResult = await _userRepository.UpdateUser(user);
            if (updateResult == null)
            {
                Console.WriteLine($"❌ Failed to update user profile in database for UserId: {request.UserId}");
                return new ApiResponse 
                { 
                    Success = false, 
                    Message = "Failed to update user profile in database." 
                };
            }

            List<TagEntity> tagEntities = new();
            if (user.favourite_type != null && user.favourite_type.Any())
            {
                tagEntities = await _tagRepository.GetTagsByIdsAsync(user.favourite_type);
            }
          
            var response = new UpdateUserProfileReponse
            {
                UserId = user.id,
                DisplayName = user.displayname,
                Bio = user.bio,
                AvatarUrl = user.avata_url, // ✅ Sử dụng đúng field name từ database: avata_url
                CoverUrl = user.cover_url
            };


            var apiResponse = new ApiResponse 
            { 
                Success = true, 
                Message = "User profile updated successfully.", 
                Data = response 
            };
            
            return apiResponse;
        }
    }
}
