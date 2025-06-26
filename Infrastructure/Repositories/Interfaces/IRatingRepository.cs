using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IRatingRepository
    {
        Task<RatingEntity> CreateAsync(RatingEntity rating);
        Task<RatingEntity> UpdateAsync(RatingEntity rating);
        Task<bool> DeleteAsync(string id);
        Task<RatingEntity> GetByIdAsync(string id);
        Task<List<RatingEntity>> GetAllAsync();
        Task<List<RatingEntity>> GetByNovelIdAsync(string novelId);
        Task<RatingEntity> GetByUserAndNovelAsync(string userId, string novelId);
        Task<double> GetAverageRatingByNovelIdAsync(string novelId);
        Task<int> GetRatingCountByNovelIdAsync(string novelId);
    }
}
