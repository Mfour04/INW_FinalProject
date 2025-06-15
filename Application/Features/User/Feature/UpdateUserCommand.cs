using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User.Feature
{
    public class UpdateUserCommand : IRequest<ApiResponse>
    {
        public UpdateUserReponse UserUpdate { get; set; }
        public IFormFile AvataFile { get; set; }
    }
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICloudDinaryService _cloudDinaryService;
        public UpdateUserHandler(IUserRepository userRepository, IMapper mapper, ICloudDinaryService cloudDinaryService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _cloudDinaryService = cloudDinaryService;
        }
        public async Task<ApiResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserUpdate.UserId);
            if(user == null)
                return new ApiResponse { Success = false, Message = "User not found." };
            user.displayname = request.UserUpdate.DisplayName;
            user.displayname_normalized = request.UserUpdate.DisplayName.ToLowerInvariant();
            user.updated_at = DateTime.UtcNow.Ticks;
            user.badge_id = request.UserUpdate.BadgeId;
            if (request.AvataFile != null && request.AvataFile.Length > 0)
            {
                var uploadAvater = await _cloudDinaryService.UploadImagesAsync(request.AvataFile);
                user.avata_url = uploadAvater;
            }

            await _userRepository.UpdateUser(user);
            var response = _mapper.Map<UpdateUserReponse>(user);

            return new ApiResponse 
            { 
                Success = true, 
                Message = "Profile updated successfully.",
                Data = response
            };
        }
    }
}
