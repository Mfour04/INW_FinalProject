using Application.Commands.Users;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.User
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        public RegisterUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<ApiResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var registerRequest = request.RegisterRequest;
            if (registerRequest == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Invalid Register"
                };
            }

            var existingUser = await _userRepository.GetByEmail(registerRequest.Email);
            if (existingUser != null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            var user = new Users
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.PasswordHash),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateUser(user);
            return new ApiResponse
            {
                Success = true,
                Message = "Register successfull",
                Data = user
            };
        }
    }
}
