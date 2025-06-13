using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User.Feature
{
    public class VerifyUserCommand: IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }
    public class VerifyUserHandler : IRequestHandler<VerifyUserCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;

        public VerifyUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<ApiResponse> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
                return new ApiResponse { Success = false, Message = "User not found." };
            
            if(user.is_verified)
                return new ApiResponse { Success = false, Message = "User already verified." };

            user.is_verified = true;
            user.updated_at = DateTime.UtcNow.Ticks;

            await _userRepository.UpdateUser(user);
            return new ApiResponse { Success = true, Message = "Email Verified Successfully"};

        }
    }
}
