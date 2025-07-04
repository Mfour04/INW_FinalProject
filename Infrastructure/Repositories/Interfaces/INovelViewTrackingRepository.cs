using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface INovelViewTrackingRepository
    {
        Task<NovelViewTrackingEntity?> FindByUserAndNovelAsync(string userId, string novelId);
        Task<NovelViewTrackingEntity> CreateViewTrackingAsync(NovelViewTrackingEntity entity);
        Task<NovelViewTrackingEntity> UpdateViewTrackingAsync(NovelViewTrackingEntity entity);
    }
}
