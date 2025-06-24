using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;

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

            transaction.updated_at = DateTime.Now.Ticks;

            await _transactionRepo.UpdateStatusAsync(transaction.id, PaymentStatus.Cancelled);
        }
    }
}