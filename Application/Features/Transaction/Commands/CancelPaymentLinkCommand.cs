using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Net.payOS;
using Shared.Helpers;

namespace Application.Features.Transaction.Commands
{
    public class CancelPaymentLinkCommand : IRequest
    {
        public long OrderCode { get; set; }
    }

    public class CancelPaymentLinkCommandHandler : IRequestHandler<CancelPaymentLinkCommand>
    {
        private readonly PayOS _payOS;
        private readonly ITransactionRepository _transactionRepo;

        public CancelPaymentLinkCommandHandler(PayOS payOS, ITransactionRepository transactionRepo)
        {
            _payOS = payOS;
            _transactionRepo = transactionRepo;
        }

        public async Task Handle(CancelPaymentLinkCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepo.GetByOrderCodeAsync(request.OrderCode);
            if (transaction == null || transaction.status != PaymentStatus.Pending)
                return;

            await _payOS.cancelPaymentLink(request.OrderCode, "Người dùng không hoạt động quá lâu");

            TransactionEntity updated = new()
            {
                status = PaymentStatus.Failed,
                updated_at = TimeHelper.NowTicks,
            };

            await _transactionRepo.UpdateStatusAsync(transaction.id, updated);
        }
    }
}