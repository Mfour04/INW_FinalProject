using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICommentLikeRepository
    {
        Task<CommentLikeEntity> LikeCommentAsync(CommentLikeEntity entity);
        Task<bool> UnlikeCommentAsync(string commentId, string userId);
        Task<bool> HasUserLikedCommentAsync(string commentId, string userId);
    }
}