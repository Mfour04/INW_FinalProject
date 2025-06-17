using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implements
{
    public class ReportRepository : IReportRepository
    {
        private readonly IMongoCollection<ReportEntity> _collection;
        public ReportRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("report").Wait();
            _collection = mongoDBHelper.GetCollection<ReportEntity>("report");
        }
        public async Task<ReportEntity> CreateAsync(ReportEntity report)
        {
            try
            {
                await _collection.InsertOneAsync(report);
                return report;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.id, id);
                var result = await _collection.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> ExistsAsync(string userId, ReportTypeStatus type, string targetId)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.And(
                    Builders<ReportEntity>.Filter.Eq(r => r.user_id, userId),
                    Builders<ReportEntity>.Filter.Eq(r => r.type, type)
                );

                switch(type)
                {
                    case ReportTypeStatus.UserReport:
                        filter = Builders<ReportEntity>.Filter.And(filter, Builders<ReportEntity>.Filter.Eq(r => r.member_id, targetId));
                        break;
                    case ReportTypeStatus.NovelReport:
                        filter = Builders<ReportEntity>.Filter.And(filter, Builders<ReportEntity>.Filter.Eq(r => r.novel_id, targetId));
                        break;
                    case ReportTypeStatus.ChapterReport:
                        filter = Builders<ReportEntity>.Filter.And(filter, Builders<ReportEntity>.Filter.Eq(r => r.chapter_id, targetId));
                        break;
                    case ReportTypeStatus.CommentReport:
                        filter = Builders<ReportEntity>.Filter.And(filter, Builders<ReportEntity>.Filter.Eq(r => r.comment_id, targetId));
                        break;
                    case ReportTypeStatus.ForumPostReport:
                        filter = Builders<ReportEntity>.Filter.And(filter, Builders<ReportEntity>.Filter.Eq(r => r.forum_post_id, targetId));
                        break;
                    case ReportTypeStatus.ForumCommentReport:
                        filter = Builders<ReportEntity>.Filter.And(filter, Builders<ReportEntity>.Filter.Eq(r => r.forum_comment_id, targetId));
                        break;
                }

                var count = await _collection.CountDocumentsAsync(filter);
                return count > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetAllAsync(FindCreterias findCreterias)
        {
            try
            {
                var result = await _collection
                    .Find(_ => true)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetByChapterIdAsync(FindCreterias findCreterias, string chapterId)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.chapter_id, chapterId);
                var result = await _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetByCommentIdAsync(FindCreterias findCreterias, string commentId)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.comment_id, commentId);
                var result = await _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetByForumCommentIdAsync(FindCreterias findCreterias, string forumCommentId)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.forum_comment_id, forumCommentId);
                var result = await _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetByForumPostIdAsync(FindCreterias findCreterias, string forumPostId)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.forum_post_id, forumPostId);
                var result = await _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<ReportEntity> GetByIdAsync(string id)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.id, id);
                var result = await _collection.Find(filter).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetByMemberIdAsync(FindCreterias findCreterias, string memberId)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.member_id, memberId);
                var result = await _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetByNovelIdAsync(FindCreterias findCreterias, string novelId)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.novel_id, novelId);
                var result = await _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetByStatusAsync(FindCreterias findCreterias, ReportStatus status)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.status, status);
                var result = await _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetByTypeAsync(FindCreterias findCreterias, ReportTypeStatus type)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.type, type);
                var result = await _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> GetByUserIdAsync(FindCreterias findCreterias, string userId)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.user_id, userId);
                var result = await _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<ReportEntity> UpdateAsync(ReportEntity report)
        {
            try
            {
                report.updated_at = DateTime.UtcNow.Ticks;
                var filter = Builders<ReportEntity>.Filter.Eq(r => r.id, report.id);
                var result = await _collection.ReplaceOneAsync(filter, report);
                return report;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
