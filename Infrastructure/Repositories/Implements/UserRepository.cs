using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implements
{
    public class UserRepository : BaseRepository<Users>, IUserRepository
    {
        private readonly IMongoCollection<Users> _userCollection;
        public UserRepository(InwDataContext context) : base(context, "Users")
        {
            _userCollection = context.GetCollection<Users>("Users");
        }

        public async Task<Users> CreateUser(Users users)
        {
            users.CreatedAt = DateTime.UtcNow;
            await _userCollection.InsertOneAsync(users);
            return users;
        }

        public async Task<Users> GetByEmail(string email)
        {
            var filter = Builders<Users>.Filter.Eq(u => u.Email, email);
            return await _userCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Users> GetById(string userId)
        {
            var filter = Builders<Users>.Filter.Eq(u => u.UserId, userId);
            return await _userCollection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
