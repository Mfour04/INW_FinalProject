using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity> CreateUser(UserEntity entity);
        Task<UserEntity> GetByEmail(string email);
        Task<UserEntity> GetById(string userId);
        Task<UserEntity> GetByName(string userName);
    }
}
