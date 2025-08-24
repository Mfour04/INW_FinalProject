using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<List<ReportEntity>> GetAllAsync(ReportScope? scope, ReportStatus? status, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<ReportEntity> GetByIdAsync(string id);
        Task<ReportEntity> CreateAsync(ReportEntity report);
        Task<List<ReportEntity>> GetManyByIdsAsync(List<string> ids);
        Task<ReportEntity> UpdateAsync(ReportEntity report);
        Task<List<ReportEntity>> UpdateManyAsync(List<string> ids, ReportStatus newStatus);
        Task<bool> DeleteAsync(string id);

        Task<long> CountByReporterAsync(string reporterId, long fromTicks);
        Task<bool> ExistsAsync(
            string reporterId,
            ReportScope scope,
            string? novelId,
            string? chapterId,
            string? commentId,
            string? forumPostId,
            string? forumCommentId,
            ReportReason reason,
            ReportStatus? status,
            long fromTicks);
    }
}
