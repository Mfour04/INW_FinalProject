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
                return Fail("Bank account not found.");
            }

            if (bankAccount.user_id != currentUserId)
            {
                return Fail("You are not authorized to set this bank account as default.");
            }

            if (bankAccount.is_default)
                return Fail("This bank account is already set as default.");

            var success = await _bankRepo.SetDefaultAsync(currentUserId, bankAccount.id);
            if (!success)
            {
                return Fail("Failed to update default bank account.");
            }

            return new ApiResponse
            {
                Success = success,
                Message = "Default bank account updated successfully."
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}