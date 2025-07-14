using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Helpers;

namespace Application.Features.Transaction.Commands
{
    public class ConfirmTransactionCommand : IRequest
    {
        public long OrderCode { get; set; }
    }

    public class ConfirmTransactionCommandHandler : IRequestHandler<ConfirmTransactionCommand>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IUserRepository _userRepo;

        public ConfirmTransactionCommandHandler(
              ITransactionRepository transactionRepo,
              IUserRepository userRepo)
        {
            _transactionRepo = transactionRepo;
            _userRepo = userRepo;
        }

        public async Task Handle(ConfirmTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepo.GetByOrderCodeAsync(request.OrderCode);
            if (transaction == null || transaction.status == PaymentStatus.Completed)
                return;

            transaction.completed_at = TimeHelper.NowTicks;

            await _transactionRepo.UpdateStatusAsync(transaction.id, PaymentStatus.Completed);

            if (transaction.type == PaymentType.TopUp)
            {
                await _userRepo.IncreaseCoinAsync(transaction.user_id, transaction.amount);
            }
        }
    }
}
