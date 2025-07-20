using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ITransactionLogRepository
    {
        Task AddAsync(TransactionLogEntity entity);
        Task<List<TransactionLogEntity>> GetLogsByTransactionIdsAsync(List<string> transactionIds);
        Task<TransactionLogEntity> GetLogByTransactionIdAsync(string transactionId);
    }
}