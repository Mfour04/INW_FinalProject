using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.BankAccount.Commands
{
    public class SetDefaultBankAccountCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }


    public class SetDefaultBankAccountCommandHandler : IRequestHandler<SetDefaultBankAccountCommand, ApiResponse>
    {
        private readonly IUserBankAccountRepository _bankRepo;
        private readonly ICurrentUserService _currentUser;

        public SetDefaultBankAccountCommandHandler(
            IUserBankAccountRepository bankRepo,
            ICurrentUserService currentUser)
        {
            _bankRepo = bankRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(SetDefaultBankAccountCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId;

            var bankAccount = await _bankRepo.GetByIdAsync(request.Id);
            if (bankAccount == null)
            {
                return Fail("Không tìm thấy tài khoản ngân hàng.");
            }

            if (bankAccount.user_id != currentUserId)
            {
                return Fail("Bạn không có quyền đặt tài khoản này làm mặc định.");
            }

            if (bankAccount.is_default)
                return Fail("Tài khoản ngân hàng này đã được đặt làm mặc định.");

            var success = await _bankRepo.SetDefaultAsync(currentUserId, bankAccount.id);
            if (!success)
            {
                return Fail("Cập nhật tài khoản ngân hàng mặc định thất bại.");
            }

            return new ApiResponse
            {
                Success = success,
                Message = "Cập nhật tài khoản ngân hàng mặc định thành công."
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}