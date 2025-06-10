using Domain.Entities.System;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IOwnershipRepository
    {
        Task<List<PurchaserEntity>> GetAllOwnerShipAsync(FindCreterias creterias);
        Task<PurchaserEntity> GetByOwnerShipIdAsync(string ownershipId);
        Task<PurchaserEntity> CreateOwnerShipAsync(PurchaserEntity entity);
        Task<PurchaserEntity> UpdateOwnerShipAsync(PurchaserEntity entity);
        Task<bool> DeleteOwnerShipAsync(string id);
        Task<bool> HasFullNovelOwnershipAsync(string userId, string novelId);
        Task<bool> HasChapterOwnershipAsync(string userId, string novelId, string chapterId);
        Task<bool> HasAnyChapterOwnershipAsync(string userId, string novelId);
        Task<List<string>> GetOwnedChapterIdsAsync(string userId, string novelId);
    }
}
