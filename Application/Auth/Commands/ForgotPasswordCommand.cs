using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.SystemHelpers.TokenGenerate;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.Auth.Commands
{
    public class ForgotPasswordCommand : IRequest<ApiResponse>
    {
        public string Email { get; set; }
    }
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly JwtHelpers _jwtHelpers;
        public ForgotPasswordCommandHandler(IUserRepository userRepository, JwtHelpers jwtHelpers, IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtHelpers = jwtHelpers;
            _emailService = emailService;
        }
        public async Task<ApiResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByEmail(request.Email);
                if (user == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Email không tồn tại trong hệ thống"
                    };
                }
                var claim = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.id),
                    new Claim("Email", user.email),
                    new Claim("Type", "PasswordReset")
                };

                var resetToken = _jwtHelpers.GenerateResetToken(claim, 15);

                var resetLink = $"http://localhost:5173/reset-password?token={resetToken}";

                var emailBody = CreateResetPasswordEmailBody(user.displayname, resetLink);

                await _emailService.SendEmailAsync(user.email, "Đặt lại mật khẩu - Inkwave Library", emailBody);
                return new ApiResponse
                {
                    Success = true,
                    Message = "Đã gửi email đặt lại mật khẩu. Vui lòng kiểm tra hộp thư của bạn."
                };
            }
            catch
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi gửi email đặt lại mật khẩu. Vui lòng thử lại sau."
                };
            }
        }
        private string CreateResetPasswordEmailBody(string displayName, string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Đặt lại mật khẩu</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>Xin chào {displayName},</h2>
                        
                        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản Inkwave Library của bạn.</p>
                        
                        <p>Vui lòng nhấp vào nút bên dưới để đặt lại mật khẩu:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' 
                               style='background-color: #3498db; color: white; padding: 12px 30px; 
                                      text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Đặt lại mật khẩu
                            </a>
                        </div>
                        
                        <p>Hoặc copy và dán đường link sau vào trình duyệt:</p>
                        <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px;'>
                            {resetLink}
                        </p>
                        
                        <p><strong>Lưu ý:</strong></p>
                        <ul>
                            <li>Link này chỉ có hiệu lực trong <strong>15 phút</strong></li>
                            <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                            <li>Để bảo mật, không chia sẻ link này với bất kỳ ai</li>
                        </ul>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        
                        <p style='color: #7f8c8d; font-size: 14px;'>
                            Trân trọng,<br>
                            <strong>Đội ngũ Inkwave Library</strong>
                        </p>
                    </div>
                </body>
                </html>";
        }
    }
}
