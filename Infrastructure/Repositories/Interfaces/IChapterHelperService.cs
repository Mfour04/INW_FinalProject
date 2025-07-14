using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IChapterHelperService
    {
        Task<string> GetChapterAuthorIdAsync(string chapterId);
        Task ProcessViewAsync(string chapterId, string userId);
    }
}
