using Application.Services.Interfaces;
using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;
using Shared.Helpers;
using static Domain.Entities.UserEntity;

namespace Application.Features.User.Feature
{
    public class UpdateUserProfileCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public IFormFile? AvataUrl { get; set; }
        public List<string> BadgeId { get; set; } = new();
        public List<TagName> FavouriteType { get; set; } = new();
    }
    public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICloudDinaryService _cloudDinaryService;
        private readonly IOpenAIService _openAIService;
        private readonly IOpenAIRepository _openAIRepository;
        public UpdateUserProfileHandler(IUserRepository userRepository, IMapper mapper
            , ICloudDinaryService cloudDinaryService, IOpenAIService openAIService, IOpenAIRepository openAIRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _cloudDinaryService = cloudDinaryService;
            _openAIService = openAIService;
            _openAIRepository = openAIRepository;   
        }
        public async Task<ApiResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserId);
            if(user == null)
                return new ApiResponse { Success = false, Message = "User not found." };
            user.displayname = request.DisplayName;
            user.displayname_unsigned = SystemHelper.RemoveDiacritics(request.DisplayName);
            user.displayname_normalized = SystemHelper.RemoveDiacritics(request.DisplayName);
            user.bio = request.Bio;
            user.updated_at = DateTime.UtcNow.Ticks;
            user.badge_id = request.BadgeId;
            user.favourite_type = request.FavouriteType;

            if (request.AvataUrl != null)
            {
                var imageAUrl = await _cloudDinaryService.UploadImagesAsync(request.AvataUrl);
                user.avata_url = imageAUrl;
            }
            var validTags = request.FavouriteType
                .Where(t => !string.IsNullOrWhiteSpace(t.name_tag))
                .ToList();
                user.favourite_type = validTags;

            var tags = user.favourite_type
                 .Select(t => t.name_tag.Trim().ToLowerInvariant())
                 .Distinct()
                 .ToList();

            if (tags.Count > 0)
            {
                var vector = await _openAIService.GetEmbeddingAsync(tags);
                await _openAIRepository.SaveUserEmbeddingAsync(user.id, vector);
            }

            await _userRepository.UpdateUser(user);
            var response = _mapper.Map<UpdateUserProfileReponse>(user);

            return new ApiResponse 
            { 
                Success = true, 
                Message = "Profile updated successfully.",
                Data = response
            };
        }
    }
}
