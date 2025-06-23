using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(TransactionEntity transaction);
        Task<TransactionEntity> GetByOrderCodeAsync(long orderCode);
        Task UpdateStatusAsync(string id, PaymentStatus newStatus);
        Task<List<TransactionEntity>> GetExpiredPendingTransactionsAsync(long timeoutTimestamp);
    }
}