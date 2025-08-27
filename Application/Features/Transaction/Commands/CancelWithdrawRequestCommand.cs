using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

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
        private readonly INotificationService _notificationService;

        public CancelWithdrawRequestCommandHandler(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse> Handle(CancelWithdrawRequestCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);

            if (transaction == null)
                return Fail("Không tìm thấy giao dịch.");

            if (transaction.requester_id != request.UserId)
                return Fail("Không có quyền thực hiện.");

            if (transaction.type != PaymentType.WithdrawCoin)
                return Fail("Giao dịch không phải rút coin.");

            if (transaction.status != PaymentStatus.Pending)
                return Fail("Chỉ những giao dịch rút đang chờ mới có thể hủy.");

            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
                return Fail("Không tìm thấy người dùng.");

            // huỷ: trả block coin
            user.block_coin -= transaction.amount;

            TransactionEntity updated = new()
            {
                status = PaymentStatus.Cancelled,
                updated_at = TimeHelper.NowTicks,
            };

            await _userRepository.UpdateUserCoin(user.id, user.coin, user.block_coin);
            await _transactionRepository.UpdateStatusAsync(transaction.id, updated);

            await _notificationService.SendNotificationToUsersAsync(
            new[] { user.id },
            $"Bạn đã hủy yêu cầu rút {transaction.amount} coin.",
            NotificationType.WithdrawCancelled
            );

            return new ApiResponse
            {
                Success = true,
                Message = "Yêu cầu rút tiền đã bị hủy thành công.",
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