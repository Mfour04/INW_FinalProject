using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Transaction.Commands
{
    public class WithdrawRequestCommand : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public int CoinAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
    }

    public class WithdrawRequestCommandHandler : IRequestHandler<WithdrawRequestCommand, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;

        public WithdrawRequestCommandHandler(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(WithdrawRequestCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserId);

            var availableCoin = user.coin - user.block_coin;

            if (availableCoin < request.CoinAmount)
                return new ApiResponse { Success = false, Message = "Insufficient available coin." };

            // block coin
            user.block_coin += request.CoinAmount;
            await _userRepository.UpdateUserCoin(user.id, user.coin, user.block_coin);

            TransactionEntity transaction = new()
            {
                id = SystemHelper.RandomId(),
                user_id = request.UserId,
                amount = request.CoinAmount,
                type = PaymentType.WithdrawCoin,
                payment_method = request.PaymentMethod,
                status = PaymentStatus.Pending,
                completed_at = 0,
                bank_account_name = request.BankAccountName,
                bank_account_number = request.BankAccountNumber,
                created_at = DateTime.Now.Ticks,
                updated_at = 0
            };

            await _transactionRepository.AddAsync(transaction);

            return new ApiResponse
            {
                Success = true,
                Message = "Withdraw request submitted successfully.",
                Data = transaction
            };
        }
    }
}