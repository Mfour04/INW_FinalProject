using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.SystemHelpers.TokenGenerate;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Auth.Commands
{
    public class ResetPasswordCommand : IRequest<ApiResponse>
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelpers _jwtHelpers;
        public ResetPasswordCommandHandler(IUserRepository userRepository, JwtHelpers jwtHelpers)
        {
            _userRepository = userRepository;
            _jwtHelpers = jwtHelpers;
        }
        public async Task<ApiResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Token không hợp lệ."
                    };
                }
                if (string.IsNullOrEmpty(request.NewPassword))
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Mật khẩu mới không được để trống."
                    };
                }
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Mật khẩu xác nhận không khớp."
                    };
                }
                if (request.NewPassword.Length < 6)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Mật khẩu phải có ít nhất 6 ký tự."
                    };
                }

                var jwtToken = _jwtHelpers.Verify(request.Token);
                if (jwtToken == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc đã hết hạn"
                    };
                }

                var tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "Type")?.Value;
                if(tokenType != "PasswordReset")
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Token không hợp lệ. Vui lòng thử lại."
                    };
                }

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Token không chứa thông tin user"
                    };
                }

                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Người dùng không tồn tại"
                    };
                }
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.password = hashedPassword;
                await _userRepository.UpdateUser(user);

                return new ApiResponse
                {
                    Success = true,
                    Message = "Đặt lại mật khẩu thành công"
                };
            }
            catch
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi đặt lại mật khẩu. Vui lòng thử lại sau."
                };
            }
        }
    }
}
