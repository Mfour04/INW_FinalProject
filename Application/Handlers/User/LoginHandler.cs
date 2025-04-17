using Application.Commands.Users;
using Application.Exceptions;
using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using Shared.SystemHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.User
{
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
            var loginRequest = request.LoginRequest;

            if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.PasswordHash))
            {
                throw new ApiException("Email and Password are required.");
            }

            var user = await _userRepository.GetByEmail(loginRequest.Email);
            if (user == null)
            {
                throw new ApiException("Invalid email or password.");
            }

            // Kiểm tra mật khẩu
            var isValidPassword = BCrypt.Net.BCrypt.Verify(loginRequest.PasswordHash, user.PasswordHash);
            if (!isValidPassword)
            {
                throw new ApiException("Invalid email or password.");
            }

            var token = _jwtHelpers.Generate(user);

            var userResponse = _mapper.Map<UserRespone>(user);

            return new ApiResponse
            {
                Success = true,
                Message = "Login successful.",
                Data = new
                {
                    Token = token,
                    User = userResponse
                }
            };
        }
    }
}
