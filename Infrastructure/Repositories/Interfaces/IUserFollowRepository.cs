using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserFollowRepository
    {
        Task<UserFollowEntity> FollowAsync(UserFollowEntity entity);
        Task<bool> UnfollowAsync(string followerId, string followingId);
        Task<bool> IsFollowingAsync(string followerId, string followingId);
        Task<List<UserFollowEntity>> GetFollowingAsync(string followerId);
        Task<List<UserFollowEntity>> GetFollowersAsync(string followingId);
    }
}