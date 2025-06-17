using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IForumPostLikeRepository
    {
        Task<ForumPostLikeEntity> LikePostAsync(ForumPostLikeEntity entity);
        Task<bool> UnlikePostAsync(string postId, string userId);
        Task<bool> HasUserLikedPostAsync(string postId, string userId);
    }
}