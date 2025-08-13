using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;

namespace Infrastructure.Repositories.Implements
{
    public class UserBankAccountRepository : IUserBankAccountRepository
    {
        private readonly IMongoCollection<UserBankAccountEntity> _collection;

        public UserBankAccountRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("user_bank_account").Wait();
            _collection = mongoDBHelper.GetCollection<UserBankAccountEntity>("user_bank_account");
        }

        /// <summary>
        /// Lấy thông tin tài khoản ngân hàng theo Id
        /// </summary>
        public async Task<UserBankAccountEntity> GetByIdAsync(string id)
        {
            try
            {
                var filter = Builders<UserBankAccountEntity>.Filter.Eq(x => x.id, id);
                var result = await _collection.Find(filter).FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<UserBankAccountEntity>> GetByIdsAsync(List<string> ids)
        {
            try
            {
                if (ids == null || ids.Count == 0)
                    return new List<UserBankAccountEntity>();

                return await _collection
                    .Find(a => ids.Contains(a.id))
                    .ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách tài khoản ngân hàng của 1 user
        /// </summary>
        public async Task<List<UserBankAccountEntity>> GetByUserAsync(string userId)
        {
            try
            {
                var filter = Builders<UserBankAccountEntity>.Filter.Eq(x => x.user_id, userId);
                var result = await _collection.Find(filter).ToListAsync();

                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Thêm tài khoản ngân hàng mới cho user
        /// </summary>
        public async Task AddAsync(UserBankAccountEntity entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Xóa tài khoản ngân hàng
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var filter = Builders<UserBankAccountEntity>.Filter.Eq(x => x.id, id);
                var result = await _collection.DeleteOneAsync(filter);

                return result.DeletedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Đặt một tài khoản làm mặc định và hủy mặc định các tài khoản khác của user
        /// </summary>
        public async Task<bool> SetDefaultAsync(string userId, string accountId)
        {
            try
            {
                var builder = Builders<UserBankAccountEntity>.Filter;

                var unsetFilter = builder.And(
                    builder.Eq(x => x.user_id, userId),
                    builder.Eq(x => x.is_default, true)
                );
                var unsetUpdate = Builders<UserBankAccountEntity>.Update.Set(x => x.is_default, false);
                await _collection.UpdateOneAsync(unsetFilter, unsetUpdate);

                var setFilter = builder.And(
                    builder.Eq(x => x.user_id, userId),
                    builder.Eq(x => x.id, accountId)
                );

                var setUpdate = Builders<UserBankAccountEntity>.Update.Set(x => x.is_default, true);
                var result = await _collection.UpdateOneAsync(setFilter, setUpdate);

                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}