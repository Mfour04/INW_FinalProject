using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Helpers;

namespace Application.Features.Transaction.Commands
{
    public class CancelTransactionCommand : IRequest
    {
        public long OrderCode { get; set; }
    }

    public class CancelTransactionCommandHandler : IRequestHandler<CancelTransactionCommand>
    {
        private readonly ITransactionRepository _transactionRepo;

        public CancelTransactionCommandHandler(ITransactionRepository transactionRepo)
        {
            _transactionRepo = transactionRepo;
        }

        public async Task Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepo.GetByOrderCodeAsync(request.OrderCode);
            if (transaction == null || transaction.status == PaymentStatus.Cancelled)
                return;

            TransactionEntity updated = new()
            {
                status = PaymentStatus.Cancelled,
                updated_at = TimeHelper.NowTicks,
            };

            await _transactionRepo.UpdateStatusAsync(transaction.id, updated);
        }
    }
}