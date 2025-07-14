using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Transaction.Commands
{
    public class ProcessWithdrawRequestCommand : IRequest<ApiResponse>
    {
        public string TransactionId { get; set; }
        public bool IsApproved { get; set; }
    }

    public class ProcessWithdrawRequestCommandCommandHandler : IRequestHandler<ProcessWithdrawRequestCommand, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;

        public ProcessWithdrawRequestCommandCommandHandler(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(ProcessWithdrawRequestCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);

            if (transaction == null)
                return Fail("Transaction not found.");

            if (transaction.type != PaymentType.WithdrawCoin)
                return Fail("Not a withdraw transaction.");

            if (transaction.status != PaymentStatus.Pending)
                return Fail("Transaction already processed.");

            var user = await _userRepository.GetById(transaction.user_id);

            if (request.IsApproved)
            {
                // admin approve
                user.coin -= transaction.amount;
                user.block_coin -= transaction.amount;

                transaction.status = PaymentStatus.Completed;
                transaction.completed_at = TimeHelper.NowTicks;

                await _userRepository.UpdateUserCoin(user.id, user.coin, user.block_coin);
                await _transactionRepository.UpdateStatusAsync(transaction.id, PaymentStatus.Completed);

                return new ApiResponse
                {
                    Success = true,
                    Message = "Withdraw approved and coin deducted successfully.",
                    Data = transaction
                };
            }
            else
            {
                // admin deny
                user.block_coin -= transaction.amount;

                transaction.status = PaymentStatus.Rejected;
                transaction.updated_at = TimeHelper.NowTicks;

                await _userRepository.UpdateUserCoin(user.id, user.coin, user.block_coin);
                await _transactionRepository.UpdateStatusAsync(transaction.id, PaymentStatus.Completed);

                return new ApiResponse
                {
                    Success = true,
                    Message = "Withdraw denied and coin unblocked successfully.",
                    Data = transaction
                };
            }
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}