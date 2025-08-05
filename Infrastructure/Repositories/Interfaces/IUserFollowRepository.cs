using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserFollowRepository
    {
        Task<UserFollowEntity> FollowAsync(UserFollowEntity entity);
        Task<bool> UnfollowAsync(string actorId, string targetId);
        Task<bool> IsFollowingAsync(string actorId, string targetId);
        Task<List<string>> GetFollowingIdsOfUserAsync(string userId); 
        Task<List<string>> GetFollowerIdsOfUserAsync(string userId);
    }
}