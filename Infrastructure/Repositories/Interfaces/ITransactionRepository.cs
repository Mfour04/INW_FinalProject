using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<List<TransactionEntity>> GetAllAsync(PaymentType? type, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task AddAsync(TransactionEntity transaction);
        Task<TransactionEntity> GetByOrderCodeAsync(long orderCode);
        Task UpdateStatusAsync(string id, PaymentStatus newStatus);
        Task<List<TransactionEntity>> GetExpiredPendingTransactionsAsync(long timeoutTimestamp);
        Task<List<TransactionEntity>> GetUserTransactionsAsync(string userId, PaymentType? type, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<TransactionEntity> GetByIdAsync(string id);
        Task<List<TransactionEntity>> GetPendingWithdrawRequestsAsync(FindCreterias creterias, List<SortCreterias> sortCreterias);
    }
}