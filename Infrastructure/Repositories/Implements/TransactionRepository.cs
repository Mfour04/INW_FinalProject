using Domain.Entities;
using Domain.Enums;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IMongoCollection<TransactionEntity> _collection;

        public TransactionRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("transactions").Wait();
            _collection = mongoDBHelper.GetCollection<TransactionEntity>("transactions");
        }

        public async Task AddAsync(TransactionEntity transaction)
        {
            try
            {
                await _collection.InsertOneAsync(transaction);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<TransactionEntity> GetByOrderCodeAsync(long orderCode)
        {
            try
            {
                return await _collection.Find(t => t.id == orderCode.ToString()).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task UpdateStatusAsync(string id, PaymentStatus newStatus)
        {
            try
            {
                var update = Builders<TransactionEntity>.Update.Set(t => t.status, newStatus);
                await _collection.UpdateOneAsync(t => t.id == id, update);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<TransactionEntity>> GetExpiredPendingTransactionsAsync(long timeoutTimestamp)
        {

            try
            {
                var result = await _collection.Find(t => t.status == PaymentStatus.Pending
                    && t.created_at < timeoutTimestamp).ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}