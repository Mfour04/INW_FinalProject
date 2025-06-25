using Domain.Entities;
using Domain.Entities.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IReadingProcessRepository
    {
        Task<ReadingProcessEntity> GetByIdAsync(string id);
        Task<ReadingProcessEntity> GetByUserAndNovelAsync(string userId, string novelId);
        Task<List<ReadingProcessEntity>> GetReadingHistoryAsync(FindCreterias findCreterias ,string userId);
        Task<ReadingProcessEntity> CreateAsync(ReadingProcessEntity entity);
        Task<ReadingProcessEntity> UpdateAsync(ReadingProcessEntity entity);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string userId, string novelId);
    }
}
