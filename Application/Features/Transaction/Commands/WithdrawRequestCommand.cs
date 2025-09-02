using Application.Services.Interfaces;
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
        public string BankAccountId { get; set; }
    }

    public class WithdrawRequestCommandHandler : IRequestHandler<WithdrawRequestCommand, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public WithdrawRequestCommandHandler(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse> Handle(WithdrawRequestCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserId);

            var availableCoin = user.coin - user.block_coin;

            if (availableCoin < request.CoinAmount)
                return new ApiResponse { Success = false, Message = "Không đủ tiền có sẵn." };

            // block coin
            user.block_coin += request.CoinAmount;
            await _userRepository.UpdateUserCoin(user.id, user.coin, user.block_coin);

            TransactionEntity transaction = new()
            {
                id = SystemHelper.RandomId(),
                requester_id = request.UserId,
                amount = request.CoinAmount,
                type = PaymentType.WithdrawCoin,
                payment_method = "Banking",
                status = PaymentStatus.Pending,
                completed_at = 0,
                bank_account_id = request.BankAccountId,
                created_at = TimeHelper.NowTicks,
                updated_at = 0
            };

            await _transactionRepository.AddAsync(transaction);
            await _notificationService.SendNotificationToUsersAsync(
                new[] { user.id },
                $"Bạn đã gửi yêu cầu rút {request.CoinAmount} coin. Vui lòng chờ admin duyệt.",
                NotificationType.WithdrawRequest
            );

            var admins = await _userRepository.GetManyAdmin();
            var adminIds = admins.Select(a => a.id).ToArray();

            await _notificationService.SendNotificationToUsersAsync(
                adminIds,
                $"{user.displayname} đã gửi yêu cầu rút {request.CoinAmount} coin.",
                NotificationType.WithdrawRequest
            );
            return new ApiResponse
            {
                Success = true,
                Message = "Yêu cầu rút tiền đã được gửi thành công.",
                Data = transaction
            };
        }
    }
}