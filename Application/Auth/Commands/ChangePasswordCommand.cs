using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Auth.Commands
{
    public class ChangePasswordCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
    
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;

        public ChangePasswordCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Mật khẩu cũ và mới không được để trống."
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

                var user = await _userRepository.GetById(request.UserId);
                if (user == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Người dùng không tồn tại."
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.password))
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Mật khẩu cũ không đúng."
                    };
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.password = hashedPassword;
                await _userRepository.UpdateUser(user);

                return new ApiResponse
                {
                    Success = true,
                    Message = "Đổi mật khẩu thành công."
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Đã xảy ra lỗi: {ex.Message}"
                };
            }
        }
    }
}
