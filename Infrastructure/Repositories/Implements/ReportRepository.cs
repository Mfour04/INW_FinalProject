using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;

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

        public async Task<List<ReportEntity>> GetAllAsync(
            ReportScope? scope,
            ReportStatus? status,
            FindCreterias creterias,
            List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<ReportEntity>.Filter;
                var filter = builder.Empty;

                if (scope.HasValue)
                {
                    filter &= builder.Eq(t => t.scope, scope.Value);
                }

                if (status.HasValue)
                {
                    filter &= builder.Eq(t => t.status, status.Value);
                }

                var query = _collection.Find(filter);

                var sortBuilder = Builders<ReportEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<ReportEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<ReportEntity>? sortDef = criterion.Field switch
                    {
                        "created_at" => criterion.IsDescending
                            ? sortBuilder.Descending(t => t.created_at)
                            : sortBuilder.Ascending(t => t.created_at),

                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Any())
                {
                    query = query.Sort(sortBuilder.Combine(sortDefinitions));
                }

                query = query
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                return await query.ToListAsync();
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

        public async Task<bool> UpdateAsync(string id, ReportEntity entity)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Eq(x => x.id, id);

                var report = await _collection.Find(filter).FirstOrDefaultAsync();

                var update = Builders<ReportEntity>
                    .Update.Set(x => x.moderator_id, entity.moderator_id ?? report.moderator_id)
                    .Set(x => x.moderator_note, entity.moderator_note ?? report.moderator_note)
                    .Set(x => x.status, entity?.status ?? report.status)
                    .Set(x => x.action, entity?.action ?? report.action)
                    .Set(x => x.moderated_at, entity?.moderated_at ?? 0)
                    .Set(x => x.updated_at, TimeHelper.NowTicks);

                var updated = await _collection.FindOneAndUpdateAsync(
                  filter,
                  update,
                  new FindOneAndUpdateOptions<ReportEntity>
                  {
                      ReturnDocument = ReturnDocument.After,
                  }
              );

                return updated != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReportEntity>> UpdateManyAsync(List<string> ids, ReportStatus newStatus)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.In(r => r.id, ids);
                var update = Builders<ReportEntity>.Update
                    .Set(r => r.status, newStatus)
                    .Set(r => r.updated_at, TimeHelper.NowTicks);
                await _collection.UpdateManyAsync(filter, update);
                return await _collection.Find(filter).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<long> CountAsync(ReportScope? scope, ReportStatus? status)
        {
            try
            {
                var filter = Builders<ReportEntity>.Filter.Empty;

                if (scope.HasValue)
                {
                    filter &= Builders<ReportEntity>.Filter.Eq(x => x.scope, scope.Value);
                }

                if (status.HasValue)
                {
                    filter &= Builders<ReportEntity>.Filter.Eq(x => x.status, status.Value);
                }

                return await _collection.CountDocumentsAsync(filter);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<long> CountByReporterAsync(string reporterId, long fromTicks)
        {
            try
            {
                var f = Builders<ReportEntity>.Filter.And(
                    Builders<ReportEntity>.Filter.Eq(x => x.reporter_id, reporterId),
                    Builders<ReportEntity>.Filter.Gte(x => x.created_at, fromTicks)
                    );

                return await _collection.CountDocumentsAsync(f);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> ExistsAsync(
            string reporterId,
            ReportScope scope,
            string? novelId,
            string? chapterId,
            string? commentId,
            string? forumPostId,
            string? forumCommentId,
            ReportReason reason,
            ReportStatus? status,
            long fromTicks)
        {
            try
            {
                string? targetId = scope switch
                {
                    ReportScope.Novel => novelId,
                    ReportScope.Chapter => chapterId,
                    ReportScope.Comment => commentId,
                    ReportScope.ForumPost => forumPostId,
                    ReportScope.ForumComment => forumCommentId,
                    _ => null
                };
                if (string.IsNullOrWhiteSpace(targetId)) return false;

                var fb = Builders<ReportEntity>.Filter;
                var filters = new List<FilterDefinition<ReportEntity>>
            {
                fb.Eq(x => x.reporter_id, reporterId),
                fb.Eq(x => x.scope, scope),
                fb.Eq(x => x.reason, reason),
                fb.Gte(x => x.created_at, fromTicks)
            };
                if (status.HasValue)
                    filters.Add(fb.Eq(x => x.status, status.Value));

                switch (scope)
                {
                    case ReportScope.Novel:
                        filters.Add(fb.Eq(x => x.novel_id, targetId));
                        break;
                    case ReportScope.Chapter:
                        filters.Add(fb.Eq(x => x.chapter_id, targetId));
                        break;
                    case ReportScope.Comment:
                        filters.Add(fb.Eq(x => x.comment_id, targetId));
                        break;
                    case ReportScope.ForumPost:
                        filters.Add(fb.Eq(x => x.forum_post_id, targetId));
                        break;
                    case ReportScope.ForumComment:
                        filters.Add(fb.Eq(x => x.forum_comment_id, targetId));
                        break;
                }

                var filter = fb.And(filters);

                var exists = await _collection.Find(filter)
                                       .Project(x => x.id)
                                       .Limit(1)
                                       .FirstOrDefaultAsync();

                return exists != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

    }
}
