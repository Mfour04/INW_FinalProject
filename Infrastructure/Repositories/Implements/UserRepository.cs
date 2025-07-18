using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;
using SharpCompress.Common;

namespace Infrastructure.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> _collection;

        public UserRepository(MongoDBHelper mongoDBHelper)
        {
            // Tạo collection nếu chưa có
            mongoDBHelper.CreateCollectionIfNotExistsAsync("user").Wait();

            // Gán collection
            _collection = mongoDBHelper.GetCollection<UserEntity>("user");
        }

        public async Task<UserEntity> CreateUser(UserEntity entity)
        {
            try
            {
                entity.displayname_normalized = SystemHelper.RemoveDiacritics(entity.displayname);
                await _collection.InsertOneAsync(entity);

                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<UserEntity> GetByEmail(string email)
        {
            try
            {
                var result = await _collection.Find(x => x.email == email).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<UserEntity> GetById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must not be null or empty");
            try
            {
                var result = await _collection.Find(x => x.id == userId).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new InternalServerException("Error getting user by id: " + ex.Message);
            }
        }

        public async Task<UserEntity> GetByName(string userName)
        {
            try
            {
                var result = await _collection.Find(x => x.username == userName).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<UserEntity> UpdateUser(UserEntity entity)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(x => x.id, entity.id);
                var result = await _collection.ReplaceOneAsync(filter, entity);

                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task IncreaseCoinAsync(string userId, int amount)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(u => u.id, userId);
                var update = Builders<UserEntity>.Update.Inc(u => u.coin, amount);

                var result = await _collection.UpdateOneAsync(filter, update);
            }
            catch
            {
                throw new InternalServerException();

            }
        }

        public async Task<bool> DecreaseCoinAsync(string userId, int amount)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.And(
                    Builders<UserEntity>.Filter.Eq(u => u.id, userId),
                    Builders<UserEntity>.Filter.Gte(u => u.coin, amount)
                );

                var update = Builders<UserEntity>.Update.Inc(u => u.coin, -amount);

                var result = await _collection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> UpdateUserRoleToAdminAsync(string userId)
        {
            var filter = Builders<UserEntity>.Filter.Eq(u => u.id, userId);
            var update = Builders<UserEntity>.Update.Set(nameof(UserEntity.role), Role.Admin);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        public async Task<UserEntity?> GetFirstUserByRoleAsync(Role role)
        {
            var filter = Builders<UserEntity>.Filter.Eq(nameof(UserEntity.role), role);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<UserEntity> GetUserNameByUserId(string userId)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(x => x.id, userId);
                var projection = Builders<UserEntity>.Projection.Include(x => x.username);
                var result = await _collection.Find(filter).Project<UserEntity>(projection).FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<UserEntity>> GetUsersByIdsAsync(List<string> userIds)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.In(u => u.id, userIds);
                var result = await _collection.Find(filter).ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task UpdateUserCoin(string userId, int coin, int blockedCoin)
        {
            var filter = Builders<UserEntity>.Filter.Eq(x => x.id, userId);

            var update = Builders<UserEntity>.Update
                .Set(x => x.coin, coin)
                .Set(x => x.block_coin, blockedCoin)
                .Set(x => x.updated_at, DateTime.Now.Ticks);

            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task<(List<UserEntity> Users, int TotalCount)> GetAllUserAsync(FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<UserEntity>.Filter;
                var filtered = builder.Empty;
                var isExact = true;
                if (creterias.SearchTerm != null && creterias.SearchTerm.Count > 0)
                {
                    if (creterias.SearchTerm.Count == 1)
                    {
                        var keyword = creterias.SearchTerm[0];

                        // ✅ THAY builder.Eq thành Regex để chứa từ đó (fuzzy nhẹ)
                        filtered &= builder.Regex(
                            x => x.displayname_unsigned,
                            new BsonRegularExpression(keyword, "i")
                        );
                    }
                    else
                    {
                        // Fuzzy match: tất cả từ phải khớp
                        var regexFilters = creterias.SearchTerm.Select(term =>
                            builder.Regex(x => x.displayname_unsigned, new BsonRegularExpression(term, "i"))
                        );
                        filtered &= builder.And(regexFilters);
                    }
                }

                var query = _collection
                    .Find(filtered)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                var sortBuilder = Builders<UserEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<UserEntity>>();
                var totalCount = await _collection.CountDocumentsAsync(filtered);
                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<UserEntity>? sortDef = criterion.Field switch
                    {
                        "created_at" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.created_at)
                            : sortBuilder.Ascending(x => x.created_at),
                        "displayname_normalized" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.displayname_normalized)
                            : sortBuilder.Ascending(x => x.displayname_normalized),
                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Count >= 1)
                {
                    var combinedSort = sortBuilder.Combine(sortDefinitions);
                    query = query.Sort(combinedSort);
                }
                var novels = await query.ToListAsync();
                return (novels, (int)totalCount);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task UpdateLockvsUnLockUser(string userId, bool isbanned)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(x => x.id, userId);
                var updatelockUser = Builders<UserEntity>.Update.Set(x => x.is_banned, isbanned);
                await _collection.UpdateOneAsync(filter, updatelockUser);
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
