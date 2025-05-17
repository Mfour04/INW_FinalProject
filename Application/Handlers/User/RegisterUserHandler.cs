using Application.Commands.Users;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using Shared.SystemHelpers.TokenGenerate;
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
        private readonly JwtHelpers _jwtHelpers;
        public RegisterUserHandler(IUserRepository userRepository, JwtHelpers jwtHelpers)
        {
            _userRepository = userRepository;
            _jwtHelpers = jwtHelpers;
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

            var newUser = new Users
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.PasswordHash),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateUser(newUser);

            var token = _jwtHelpers.Generate(
                newUser.UserId.ToString(),
                newUser.Username,
                newUser.Role.ToString()
            );


            return new ApiResponse
            {
                Success = true,
                Message = "Register successfull",
                Data = newUser
            };
        }
    }
}
