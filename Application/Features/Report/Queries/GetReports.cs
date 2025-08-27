using AutoMapper;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Report;
using Shared.Helpers;

namespace Application.Features.Report.Queries
{
    public class GetReports : IRequest<ApiResponse>
    {
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;

        public ReportScope? Scope { get; set; }
        public ReportStatus? Status { get; set; }
    }

    public class GetReportsHandler : IRequestHandler<GetReports, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IForumPostRepository _forumPostRepository;
        private readonly IForumCommentRepository _forumCommentRepository;
        private readonly IMapper _mapper;

        public GetReportsHandler(
          IReportRepository reportRepository,
          IMapper mapper,
          IUserRepository userRepository,
          INovelRepository novelRepository,
          IChapterRepository chapterRepository,
          ICommentRepository commentRepository,
          IForumPostRepository forumPostRepository,
          IForumCommentRepository forumCommentRepository)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _commentRepository = commentRepository;
            _forumPostRepository = forumPostRepository;
            _forumCommentRepository = forumCommentRepository;
        }

        public async Task<ApiResponse> Handle(GetReports request, CancellationToken cancellationToken)
        {
            var find = new FindCreterias { Limit = request.Limit, Page = request.Page };
            var sort = SystemHelper.ParseSortCriteria(request.SortBy);

            var reports = await _reportRepository.GetAllAsync(request.Scope, request.Status, find, sort);
            var totalCount = await _reportRepository.CountAsync(request.Scope, request.Status);

            var novelIds = reports
                .Where(r => r.scope is ReportScope.Novel or ReportScope.Chapter or ReportScope.Comment)
                .Select(r => r.novel_id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var chapterIds = reports
                .Where(r => r.scope is ReportScope.Chapter or ReportScope.Comment)
                .Select(r => r.chapter_id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var commentIds = reports
                .Where(r => r.scope == ReportScope.Comment && !string.IsNullOrWhiteSpace(r.comment_id))
                .Select(r => r.comment_id!)
                .Distinct()
                .ToList();

            var forumPostIds = reports
                .Where(r => r.scope == ReportScope.ForumPost && !string.IsNullOrWhiteSpace(r.forum_post_id))
                .Select(r => r.forum_post_id!)
                .Distinct()
                .ToList();

            var forumCommentIds = reports
                .Where(r => r.scope == ReportScope.ForumComment && !string.IsNullOrWhiteSpace(r.forum_comment_id))
                .Select(r => r.forum_comment_id!)
                .Distinct()
                .ToList();

            var novelTitleMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var chapterTitleMap = new Dictionary<string, string>(StringComparer.Ordinal);

            var commentAuthorMap = new Dictionary<string, string>(StringComparer.Ordinal);    
            var forumPostAuthorMap = new Dictionary<string, string>(StringComparer.Ordinal);    
            var forumCommentAuthorMap = new Dictionary<string, string>(StringComparer.Ordinal); 

            foreach (var nid in novelIds)
            {
                var n = await _novelRepository.GetByNovelIdAsync(nid);
                if (n != null) novelTitleMap[nid] = n.title ?? "";
            }

            foreach (var cid in chapterIds)
            {
                var c = await _chapterRepository.GetByIdAsync(cid);
                if (c != null) chapterTitleMap[cid] = c.title ?? "";
            }

            foreach (var cid in commentIds)
            {
                var c = await _commentRepository.GetByIdAsync(cid);
                if (c != null && !string.IsNullOrWhiteSpace(c.user_id))
                {
                    commentAuthorMap[cid] = c.user_id;
                }
            }

            foreach (var fpid in forumPostIds)
            {
                var fp = await _forumPostRepository.GetByIdAsync(fpid);
                if (fp != null && !string.IsNullOrWhiteSpace(fp.user_id))
                {
                    forumPostAuthorMap[fpid] = fp.user_id;
                }
            }

            foreach (var fcid in forumCommentIds)
            {
                var fc = await _forumCommentRepository.GetByIdAsync(fcid);
                if (fc != null && !string.IsNullOrWhiteSpace(fc.user_id))
                {
                    forumCommentAuthorMap[fcid] = fc.user_id;
                }
            }

            var userIds = reports
                .Select(r => r.reporter_id)
                .Concat(reports.Where(r => !string.IsNullOrWhiteSpace(r.moderator_id)).Select(r => r.moderator_id!))
                .Concat(reports.Where(r => r.scope == ReportScope.User && !string.IsNullOrWhiteSpace(r.target_user_id)).Select(r => r.target_user_id!))
                .Concat(commentAuthorMap.Values)       
                .Concat(forumPostAuthorMap.Values)    
                .Concat(forumCommentAuthorMap.Values) 
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var userMap = new Dictionary<string, BaseReportResponse.UserResponse>(StringComparer.Ordinal);
            if (userIds.Count > 0)
            {
                var users = await _userRepository.GetUsersByIdsAsync(userIds);
                foreach (var u in users)
                {
                    userMap[u.id] = new BaseReportResponse.UserResponse
                    {
                        Id = u.id,
                        Username = u.username,
                        DisplayName = u.displayname,
                        AvatarUrl = u.avata_url
                    };
                }
            }

            BaseReportResponse.UserResponse? GetUser(string? id)
                => !string.IsNullOrWhiteSpace(id) && userMap.TryGetValue(id, out var v) ? v : null;

            var responseList = new List<object>(reports.Count);

            foreach (var r in reports)
            {
                switch (r.scope)
                {
                    case ReportScope.Novel:
                    {
                        var dto = _mapper.Map<NovelReportResponse>(r);
                        dto.Reporter = GetUser(r.reporter_id)!;
                        dto.Moderator = GetUser(r.moderator_id);

                        if (!string.IsNullOrWhiteSpace(r.novel_id) &&
                            novelTitleMap.TryGetValue(r.novel_id, out var nTitle))
                        {
                            dto.NovelTitle = nTitle;
                        }

                        responseList.Add(dto);
                        break;
                    }

                    case ReportScope.Chapter:
                    {
                        var dto = _mapper.Map<ChapterReportResponse>(r);
                        dto.Reporter = GetUser(r.reporter_id)!;
                        dto.Moderator = GetUser(r.moderator_id);

                        if (!string.IsNullOrWhiteSpace(r.chapter_id) &&
                            chapterTitleMap.TryGetValue(r.chapter_id, out var cTitle))
                        {
                            dto.ChapterTitle = cTitle;
                        }

                        if (!string.IsNullOrWhiteSpace(r.novel_id) &&
                            novelTitleMap.TryGetValue(r.novel_id, out var nTitle))
                        {
                            dto.NovelTitle = nTitle;
                        }

                        responseList.Add(dto);
                        break;
                    }

                    case ReportScope.Comment:
                    {
                        var dto = _mapper.Map<CommentReportResponse>(r);
                        dto.Reporter = GetUser(r.reporter_id)!;
                        dto.Moderator = GetUser(r.moderator_id);

                        if (!string.IsNullOrWhiteSpace(r.comment_id) &&
                            commentAuthorMap.TryGetValue(r.comment_id, out var commentAuthorId))
                        {
                            dto.CommentAuthor = GetUser(commentAuthorId);
                        }

                        responseList.Add(dto);
                        break;
                    }

                    case ReportScope.ForumPost:
                    {
                        var dto = _mapper.Map<ForumPostReportResponse>(r);
                        dto.Reporter = GetUser(r.reporter_id)!;
                        dto.Moderator = GetUser(r.moderator_id);

                        if (!string.IsNullOrWhiteSpace(r.forum_post_id) &&
                            forumPostAuthorMap.TryGetValue(r.forum_post_id, out var forumPostAuthorId))
                        {
                            dto.ForumPostAuthor = GetUser(forumPostAuthorId);
                        }

                        responseList.Add(dto);
                        break;
                    }

                    case ReportScope.ForumComment:
                    {
                        var dto = _mapper.Map<ForumCommentReportResponse>(r);
                        dto.Reporter = GetUser(r.reporter_id)!;
                        dto.Moderator = GetUser(r.moderator_id);

                        if (!string.IsNullOrWhiteSpace(r.forum_comment_id) &&
                            forumCommentAuthorMap.TryGetValue(r.forum_comment_id, out var forumCommentAuthorId))
                        {
                            dto.ForumCommentAuthor = GetUser(forumCommentAuthorId);
                        }

                        responseList.Add(dto);
                        break;
                    }

                    case ReportScope.User:
                    {
                        var dto = _mapper.Map<UserReportResponse>(r);
                        dto.Reporter = GetUser(r.reporter_id)!;
                        dto.Moderator = GetUser(r.moderator_id);
                        dto.TargetUser = GetUser(r.target_user_id);
                        responseList.Add(dto);
                        break;
                    }
                }
            }

            // ========== 5) Paging meta ==========
            var limit = request.Limit <= 0 ? int.MaxValue : request.Limit;
            var totalPages = totalCount == 0
                ? 0
                : (limit == int.MaxValue ? 1 : (int)Math.Ceiling(totalCount / (double)limit));

            return new ApiResponse
            {
                Success = true,
                Message = responseList.Count == 0 ? "No report found." : "Retrieved reports successfully.",
                Data = new
                {
                    Reports = responseList,
                    TotalReports = totalCount,
                    TotalPages = totalPages
                }
            };
        }
    }
}
