using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using Shared.Helpers;
using Shared.SystemHelpers.TokenGenerate;

namespace Application.Auth.Commands
{
    public class RegisterCommand : IRequest<ApiResponse>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
    
    public class RegisterUserHandler : IRequestHandler<RegisterCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelpers _jwtHelpers;
        public RegisterUserHandler(IUserRepository userRepository, JwtHelpers jwtHelpers)
        {
            _userRepository = userRepository;
            _jwtHelpers = jwtHelpers;
        }
        public async Task<ApiResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmail(request.Email);
            if (existingUser != null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            var newUser = new UserEntity
            {
                id = SystemHelper.RandomId(),
                user_name = request.Username,
                email = request.Email,
                password_hash = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash),
                role = Role.Reader,
                created_at = DateTime.Now.Ticks,
                updated_at = DateTime.Now.Ticks
            };

            await _userRepository.CreateUser(newUser);

            var token = _jwtHelpers.Generate(
                newUser.id.ToString(),
                newUser.user_name,
                newUser.role.ToString()
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
