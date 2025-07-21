using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;
using Shared.SystemHelpers.TokenGenerate;

namespace Application.Auth.Commands
{
    public class RegisterCommand : IRequest<ApiResponse>
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    
    public class RegisterUserHandler : IRequestHandler<RegisterCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly JwtHelpers _jwtHelpers;
        public RegisterUserHandler(IUserRepository userRepository, JwtHelpers jwtHelpers, IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtHelpers = jwtHelpers;
            _emailService = emailService;
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
                username = request.UserName,
                displayname = request.UserName,
                email = request.Email,
                password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                role = Role.User,
                is_verified = false,
                created_at = DateTime.Now.Ticks,
                updated_at = DateTime.Now.Ticks
            };

            await _userRepository.CreateUser(newUser);

            var token = _jwtHelpers.Generate(
                newUser.id.ToString(),
                newUser.username,
                newUser.role.ToString()
            );

            var verifyUrl = $"https://localhost:7242/api/Users/verify-email?token={token}";

            var emailBody = $@"
        <h3>Chào {newUser.username},</h3>
        <p>Cảm ơn bạn đã đăng ký.</p>
        <p>Vui lòng nhấn vào <a href='{verifyUrl}'>liên kết này</a> để xác thực email.</p>
        <p>Liên kết sẽ hết hạn sau 15 phút.</p>
    ";

            await _emailService.SendEmailAsync(
                newUser.email,
                "Xác thực tài khoản",
                emailBody
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
