using Domain.Entities;
using Domain.Entities.System;
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

        /// <summary>
        /// Lấy toàn bộ danh sách giao dịch, có thể lọc theo loại giao dịch.
        /// </summary>
        public async Task<List<TransactionEntity>> GetAllAsync(
            PaymentType? type,
            FindCreterias creterias,
            List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<TransactionEntity>.Filter;
                var filter = builder.Empty;

                if (type.HasValue)
                {
                    filter &= builder.Eq(t => t.type, type.Value);
                }

                var query = _collection.Find(filter);

                var sortBuilder = Builders<TransactionEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<TransactionEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<TransactionEntity>? sortDef = criterion.Field switch
                    {
                        "completed_at" => criterion.IsDescending
                            ? sortBuilder.Descending(t => t.completed_at)
                            : sortBuilder.Ascending(t => t.completed_at),

                        "amount" => criterion.IsDescending
                            ? sortBuilder.Descending(t => t.amount)
                            : sortBuilder.Ascending(t => t.amount),

                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Any())
                {
                    query = query.Sort(sortBuilder.Combine(sortDefinitions));
                }

                query = query
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
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

        /// <summary>
        /// Lấy danh sách giao dịch của người dùng theo loại giao dịch.
        /// </summary>
        public async Task<List<TransactionEntity>> GetUserTransactionsAsync(
            string userId,
            PaymentType? type,
            FindCreterias creterias,
            List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<TransactionEntity>.Filter;
                var filter = builder.Eq(t => t.user_id, userId);

                if (type.HasValue)
                {
                    filter &= builder.Eq(t => t.type, type.Value);
                }

                var query = _collection.Find(filter);

                var sortBuilder = Builders<TransactionEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<TransactionEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<TransactionEntity>? sortDef = criterion.Field switch
                    {
                        "completed_at" => criterion.IsDescending
                            ? sortBuilder.Descending(t => t.completed_at)
                            : sortBuilder.Ascending(t => t.completed_at),

                        "amount" => criterion.IsDescending
                            ? sortBuilder.Descending(t => t.amount)
                            : sortBuilder.Ascending(t => t.amount),

                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Any())
                {
                    query = query.Sort(sortBuilder.Combine(sortDefinitions));
                }

                query = query
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}