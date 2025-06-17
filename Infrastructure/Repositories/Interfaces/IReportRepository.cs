using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<ReportEntity> CreateAsync(ReportEntity report);
        Task<ReportEntity> GetByIdAsync(string id);
        Task<List<ReportEntity>> GetAllAsync(FindCreterias findCreterias);
        Task<List<ReportEntity>> GetByUserIdAsync(FindCreterias findCreterias, string userId);
        Task<List<ReportEntity>> GetByMemberIdAsync(FindCreterias findCreterias, string memberId);
        Task<List<ReportEntity>> GetByStatusAsync(FindCreterias findCreterias, ReportStatus status);
        Task<List<ReportEntity>> GetByTypeAsync(FindCreterias findCreterias, ReportTypeStatus type);
        Task<List<ReportEntity>> GetByNovelIdAsync(FindCreterias findCreterias, string novelId);
        Task<List<ReportEntity>> GetByChapterIdAsync(FindCreterias findCreterias, string chapterId);
        Task<List<ReportEntity>> GetByCommentIdAsync(FindCreterias findCreterias, string commentId);
        Task<List<ReportEntity>> GetByForumPostIdAsync(FindCreterias findCreterias, string forumPostId);
        Task<List<ReportEntity>> GetByForumCommentIdAsync(FindCreterias findCreterias, string forumCommentId);
        Task<ReportEntity> UpdateAsync(ReportEntity report);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string userId, ReportTypeStatus type, string targetId);
    }
}
