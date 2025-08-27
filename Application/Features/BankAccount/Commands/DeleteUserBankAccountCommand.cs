using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.BankAccount.Commands
{
    public class DeleteUserBankAccountCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }

    public class DeleteUserBankAccountCommandHandler : IRequestHandler<DeleteUserBankAccountCommand, ApiResponse>
    {
        private readonly IUserBankAccountRepository _bankRepo;
        private readonly ICurrentUserService _currentUser;

        public DeleteUserBankAccountCommandHandler(
            IUserBankAccountRepository bankRepo,
            ICurrentUserService currentUser)
        {
            _bankRepo = bankRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(DeleteUserBankAccountCommand request, CancellationToken cancellationToken)
        {
            var bankAccount = await _bankRepo.GetByIdAsync(request.Id);
            if (bankAccount == null)
            {
                return Fail("Tài khoản ngân hàng không tìm thấy.");
            }

            if (!_currentUser.IsAdmin() && bankAccount.user_id != _currentUser.UserId)
            {
                return Fail("Bạn không được phép xóa tài khoản ngân hàng này.");
            }

            var deleted = await _bankRepo.DeleteAsync(request.Id);
            if (!deleted)
            {
                return Fail("Không xóa được tài khoản ngân hàng của người dùng.");
            }

            return new ApiResponse
            {
                Success = deleted,
                Message = "Tài khoản ngân hàng của người dùng đã được xóa thành công."
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}