using Domain.Entities;
using Domain.Entities.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface INovelRepository
    {
        Task<List<NovelEntity>> GetAllNovelAsync(FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<NovelEntity> GetByNovelIdAsync(string novelId);
        Task<NovelEntity> CreateNovelAsync(NovelEntity entity);
        Task<NovelEntity> UpdateNovelAsync(NovelEntity entity);
        Task<bool> DeleteNovelAsync(string id);
    }
}
