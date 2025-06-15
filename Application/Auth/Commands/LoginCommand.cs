using Application.Exceptions;
using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;
using Shared.SystemHelpers.TokenGenerate;

namespace Application.Auth.Commands
{
    public class LoginCommand : IRequest<ApiResponse>
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }

    public class LoginHandler : IRequestHandler<LoginCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelpers _jwtHelpers;
        private readonly IMapper _mapper;

        public LoginHandler(IUserRepository userRepository, JwtHelpers jwtHelpers, IMapper mapper)
        {
            _userRepository = userRepository;
            _jwtHelpers = jwtHelpers;
            _mapper = mapper;
        }
        
        public async Task<ApiResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PasswordHash))
            {
                throw new ApiException("Email and Password are required.");
            }

            var user = await _userRepository.GetByEmail(request.Email);
            if (user == null)
            {
                throw new ApiException("Invalid email or password.");
            }

            // Kiểm tra mật khẩu
            var isValidPassword = BCrypt.Net.BCrypt.Verify(request.PasswordHash, user.password);
            if (!isValidPassword)
            {
                throw new ApiException("Invalid email or password.");
            }

            //var token = _jwtHelpers.Generate(user);

            var accessToken = _jwtHelpers.Generate(user.id, user.email, user.role.ToString());
            var refreshToken = _jwtHelpers.GenerateRefreshToken(user.id);


            var userResponse = _mapper.Map<UserResponse>(user);

            return new ApiResponse
            {
                Success = true,
                Message = "Login successful.",
                Data = new
                {
                    AcessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = userResponse
                }
            };
        }
    }
}
