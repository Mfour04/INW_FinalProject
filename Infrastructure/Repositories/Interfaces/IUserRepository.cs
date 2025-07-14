using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity> CreateUser(UserEntity entity);
        Task<UserEntity> GetByEmail(string email);
        Task<UserEntity> GetById(string userId);
        Task<UserEntity> GetUserNameByUserId(string userId);
        Task<UserEntity> GetByName(string userName);
        Task<UserEntity> UpdateUser(UserEntity entity);
        Task<bool> UpdateUserRoleToAdminAsync(string userId);
        Task IncreaseCoinAsync(string userId, int amount);
        Task<bool> DecreaseCoinAsync(string userId, int amount);
        Task<UserEntity?> GetFirstUserByRoleAsync(Role role);
        Task<List<UserEntity>> GetUsersByIdsAsync(List<string> userIds);
    }
}
