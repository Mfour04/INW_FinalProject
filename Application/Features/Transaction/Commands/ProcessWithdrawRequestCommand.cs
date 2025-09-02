using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Transaction.Commands
{
    public class ProcessWithdrawRequestCommand : IRequest<ApiResponse>
    {
        public string? TransactionId { get; set; }
        public string? ApproverId { get; set; }
        public bool IsApproved { get; set; }
        public string? Message { get; set; }
    }

    public class ProcessWithdrawRequestCommandCommandHandler : IRequestHandler<ProcessWithdrawRequestCommand, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionLogRepository _logRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public ProcessWithdrawRequestCommandCommandHandler(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            ITransactionLogRepository logRepository,
            INotificationService notificationService)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _logRepository = logRepository;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse> Handle(ProcessWithdrawRequestCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);

            if (transaction == null)
                return Fail("Không tìm thấy giao dịch.");

            if (transaction.type != PaymentType.WithdrawCoin)
                return Fail("Giao dịch không phải rút coin.");

            if (transaction.status != PaymentStatus.Pending)
                return Fail("Giao dịch đã được xử lý.");

            var user = await _userRepository.GetById(transaction.requester_id);

            if (!request.IsApproved && string.IsNullOrWhiteSpace(request.Message))
                return Fail("Cần cung cấp lý do từ chối.");
            string notifyMessage;
            if (request.IsApproved)
            {
                // admin approve
                user.coin -= transaction.amount;
                user.block_coin -= transaction.amount;

                TransactionEntity updated = new()
                {
                    status = PaymentStatus.Completed,
                    completed_at = TimeHelper.NowTicks,
                };

                await _userRepository.UpdateUserCoin(user.id, user.coin, user.block_coin);
                await _transactionRepository.UpdateStatusAsync(transaction.id, updated);
                notifyMessage = $"Yêu cầu rút {transaction.amount} coin của bạn đã được duyệt thành công.";
            }
            else
            {
                // admin deny
                user.block_coin -= transaction.amount;

                TransactionEntity updated = new()
                {
                    status = PaymentStatus.Rejected,
                    updated_at = TimeHelper.NowTicks,
                };

                await _userRepository.UpdateUserCoin(user.id, user.coin, user.block_coin);
                await _transactionRepository.UpdateStatusAsync(transaction.id, updated);
                notifyMessage = $"Yêu cầu rút {transaction.amount} coin của bạn đã bị từ chối. Lý do: {request.Message}";
            }

            TransactionLogEntity log = new()
            {
                id = SystemHelper.RandomId(),
                transaction_id = transaction.id,
                action_by_id = request.ApproverId,
                action_type = request.IsApproved ? "approve" : "reject",
                message = request.IsApproved ? "Approved by admin." : request.Message,
                created_at = TimeHelper.NowTicks
            };

            await _logRepository.AddAsync(log);
            await _notificationService.SendNotificationToUsersAsync(
                new[] { user.id },
                notifyMessage,
                request.IsApproved ? NotificationType.WithdrawApproved : NotificationType.WithdrawRejected
            );
            return new ApiResponse
            {
                Success = true,
                Message = request.IsApproved
                ? "Rút tiền được phê duyệt và coin đã bị trừ thành công."
                : "Rút tiền bị từ chối và coin đã được mở khóa thành công.",
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