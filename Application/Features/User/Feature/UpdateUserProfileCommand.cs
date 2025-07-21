using Application.Services.Interfaces;
using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;
using Shared.Helpers;

namespace Application.Features.User.Feature
{
    public class UpdateUserProfileCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public IFormFile? AvataUrl { get; set; }
        public List<string> BadgeId { get; set; } = new();
    }
    public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICloudDinaryService _cloudDinaryService;
        public UpdateUserProfileHandler(IUserRepository userRepository, IMapper mapper, ICloudDinaryService cloudDinaryService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _cloudDinaryService = cloudDinaryService;
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

            if(request.AvataUrl != null)
            {
                var imageAUrl = await _cloudDinaryService.UploadImagesAsync(request.AvataUrl);
                user.avata_url = imageAUrl;
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
