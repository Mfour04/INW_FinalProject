using Domain.Entities.System;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ITagRepository
    {
        Task<List<TagEntity>> GetAllTagAsync();
        Task<TagEntity> GetByTagIdAsync(string tagId);
        Task<TagEntity> CreateTagAsync(TagEntity entity);
        Task<TagEntity> UpdateTagAsync(TagEntity entity);
        Task<bool> DeleteTagAsync(string id);
        Task<List<TagEntity>> GetTagsByIdsAsync(List<string> ids);
    }
}
