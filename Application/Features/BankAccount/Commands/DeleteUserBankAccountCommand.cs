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
                return Fail("Bank account not found.");
            }

            if (!_currentUser.IsAdmin() && bankAccount.user_id != _currentUser.UserId)
            {
                return Fail("You are not authorized to delete this bank account.");
            }

            var deleted = await _bankRepo.DeleteAsync(request.Id);
            if (!deleted)
            {
                return Fail("Failed to delete user bank account.");
            }

            return new ApiResponse
            {
                Success = deleted,
                Message = "User bank account deleted successfully."
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}