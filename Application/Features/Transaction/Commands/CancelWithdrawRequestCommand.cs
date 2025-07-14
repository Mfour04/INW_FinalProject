using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Transaction.Commands
{
    public class CancelWithdrawRequestCommand : IRequest<ApiResponse>
    {
        public string TransactionId { get; set; }
        public string? UserId { get; set; }
    }

    public class CancelWithdrawRequestCommandHandler : IRequestHandler<CancelWithdrawRequestCommand, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;

        public CancelWithdrawRequestCommandHandler(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(CancelWithdrawRequestCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);

            if (transaction == null)
                return Fail("Transaction not found.");

            if (transaction.user_id != request.UserId)
                return Fail("Permission denied.");

            if (transaction.type != PaymentType.WithdrawCoin)
                return Fail("Not a withdraw transaction.");

            if (transaction.status != PaymentStatus.Pending)
                return Fail("Only pending withdraws can be cancelled.");

            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
                return Fail("User not found.");

            // huỷ: trả block coin
            user.block_coin -= transaction.amount;
            transaction.status = PaymentStatus.Cancelled;
            transaction.updated_at = DateTime.Now.Ticks;

            await _userRepository.UpdateUserCoin(user.id, user.coin, user.block_coin);
            await _transactionRepository.UpdateStatusAsync(transaction.id, PaymentStatus.Completed);

            return new ApiResponse
            {
                Success = true,
                Message = "Withdraw request cancelled successfully.",
                Data = transaction
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}