using System.Collections.Concurrent;
using AutoMapper;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Report;

namespace Application.Features.Report.Queries
{
    public class GetReportById : IRequest<ApiResponse>
    {
        public string ReportId { get; set; } = default!;
    }

    public class GetReportByIdHandler : IRequestHandler<GetReportById, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IForumPostRepository _forumPostRepository;
        private readonly IForumCommentRepository _forumCommentRepository;
        private readonly IMapper _mapper;

        public GetReportByIdHandler(
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

        public async Task<ApiResponse> Handle(GetReportById req, CancellationToken ct)
        {
            var r = await _reportRepository.GetByIdAsync(req.ReportId);
            if (r == null)
                return new ApiResponse { Success = false, Message = "Report not found." };

            var userIds = new List<string>(4);
            if (!string.IsNullOrWhiteSpace(r.reporter_id)) userIds.Add(r.reporter_id);
            if (!string.IsNullOrWhiteSpace(r.moderator_id)) userIds.Add(r.moderator_id!);
            if (r.scope == ReportScope.User && !string.IsNullOrWhiteSpace(r.target_user_id)) userIds.Add(r.target_user_id!);

           
            var userMap = new ConcurrentDictionary<string, BaseReportResponse.UserResponse>(StringComparer.Ordinal);

            if (userIds.Count > 0)
            {
                var users = await _userRepository.GetUsersByIdsAsync(userIds.Distinct().ToList());
                foreach (var u in users)
                {
                    userMap[u.id] = new BaseReportResponse.UserResponse
                    {
                        Id          = u.id,
                        Username    = u.username,
                        DisplayName = u.displayname,
                        AvatarUrl   = u.avata_url
                    };
                }
            }

            BaseReportResponse.UserResponse? GetUser(string? id)
                => !string.IsNullOrWhiteSpace(id) && userMap.TryGetValue(id!, out var v) ? v : null;

            string? novelTitle = null;
            string? chapterTitle = null;
            BaseReportResponse.UserResponse? commentAuthor = null;
            BaseReportResponse.UserResponse? forumPostAuthor = null;
            BaseReportResponse.UserResponse? forumCommentAuthor = null;

            var tasks = new List<Task>();

            if (r.scope is ReportScope.Novel or ReportScope.Chapter or ReportScope.Comment)
            {
                if (!string.IsNullOrWhiteSpace(r.novel_id))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var n = await _novelRepository.GetByNovelIdAsync(r.novel_id!);
                        novelTitle = n?.title ?? "";
                    }, ct));
                }
            }

            if (r.scope is ReportScope.Chapter or ReportScope.Comment)
            {
                if (!string.IsNullOrWhiteSpace(r.chapter_id))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var c = await _chapterRepository.GetByIdAsync(r.chapter_id!);
                        chapterTitle = c?.title ?? "";
                    }, ct));
                }
            }

            if (r.scope == ReportScope.Comment && !string.IsNullOrWhiteSpace(r.comment_id))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var c = await _commentRepository.GetByIdAsync(r.comment_id!);
                    var cid = c?.user_id;
                    if (!string.IsNullOrWhiteSpace(cid))
                    {
                        if (!userMap.ContainsKey(cid!))
                        {
                            var u = await _userRepository.GetById(cid!);
                            if (u != null)
                            {
                                userMap[u.id] = new BaseReportResponse.UserResponse
                                {
                                    Id          = u.id,
                                    Username    = u.username,
                                    DisplayName = u.displayname,
                                    AvatarUrl   = u.avata_url
                                };
                            }
                        }
                        commentAuthor = GetUser(cid);
                    }
                }, ct));
            }

            if (r.scope == ReportScope.ForumPost && !string.IsNullOrWhiteSpace(r.forum_post_id))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var p = await _forumPostRepository.GetByIdAsync(r.forum_post_id!);
                    var uid = p?.user_id;
                    if (!string.IsNullOrWhiteSpace(uid))
                    {
                        if (!userMap.ContainsKey(uid!))
                        {
                            var u = await _userRepository.GetById(uid!);
                            if (u != null)
                            {
                                userMap[u.id] = new BaseReportResponse.UserResponse
                                {
                                    Id          = u.id,
                                    Username    = u.username,
                                    DisplayName = u.displayname,
                                    AvatarUrl   = u.avata_url
                                };
                            }
                        }
                        forumPostAuthor = GetUser(uid);
                    }
                }, ct));
            }

            if (r.scope == ReportScope.ForumComment && !string.IsNullOrWhiteSpace(r.forum_comment_id))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var fc = await _forumCommentRepository.GetByIdAsync(r.forum_comment_id!);
                    var uid = fc?.user_id;
                    if (!string.IsNullOrWhiteSpace(uid))
                    {
                        if (!userMap.ContainsKey(uid!))
                        {
                            var u = await _userRepository.GetById(uid!);
                            if (u != null)
                            {
                                userMap[u.id] = new BaseReportResponse.UserResponse
                                {
                                    Id          = u.id,
                                    Username    = u.username,
                                    DisplayName = u.displayname,
                                    AvatarUrl   = u.avata_url
                                };
                            }
                        }
                        forumCommentAuthor = GetUser(uid);
                    }
                }, ct));
            }

            if (tasks.Count > 0) await Task.WhenAll(tasks);

            BaseReportResponse dto;
            switch (r.scope)
            {
                case ReportScope.Novel:
                {
                    var mapped = _mapper.Map<NovelReportResponse>(r);
                    mapped.Reporter  = GetUser(r.reporter_id);
                    mapped.Moderator = GetUser(r.moderator_id);
                    if (!string.IsNullOrWhiteSpace(novelTitle)) mapped.NovelTitle = novelTitle;
                    dto = mapped;
                    break;
                }

                case ReportScope.Chapter:
                {
                    var mapped = _mapper.Map<ChapterReportResponse>(r);
                    mapped.Reporter  = GetUser(r.reporter_id);
                    mapped.Moderator = GetUser(r.moderator_id);
                    if (!string.IsNullOrWhiteSpace(chapterTitle)) mapped.ChapterTitle = chapterTitle;
                    if (!string.IsNullOrWhiteSpace(novelTitle))   mapped.NovelTitle   = novelTitle;
                    dto = mapped;
                    break;
                }

                case ReportScope.Comment:
                {
                    var mapped = _mapper.Map<CommentReportResponse>(r);
                    mapped.Reporter      = GetUser(r.reporter_id);
                    mapped.Moderator     = GetUser(r.moderator_id);
                    mapped.CommentAuthor = commentAuthor;
                    dto = mapped;
                    break;
                }

                case ReportScope.ForumPost:
                {
                    var mapped = _mapper.Map<ForumPostReportResponse>(r);
                    mapped.Reporter        = GetUser(r.reporter_id);
                    mapped.Moderator       = GetUser(r.moderator_id);
                    mapped.ForumPostAuthor = forumPostAuthor;
                    dto = mapped;
                    break;
                }

                case ReportScope.ForumComment:
                {
                    var mapped = _mapper.Map<ForumCommentReportResponse>(r);
                    mapped.Reporter             = GetUser(r.reporter_id);
                    mapped.Moderator            = GetUser(r.moderator_id);
                    mapped.ForumCommentAuthor   = forumCommentAuthor;
                    dto = mapped;
                    break;
                }

                case ReportScope.User:
                {
                    var mapped = _mapper.Map<UserReportResponse>(r);
                    mapped.Reporter   = GetUser(r.reporter_id)!;
                    mapped.Moderator  = GetUser(r.moderator_id);
                    mapped.TargetUser = GetUser(r.target_user_id);
                    dto = mapped;
                    break;
                }

                default:
                {
                    var mapped = _mapper.Map<BaseReportResponse>(r);
                    mapped.Reporter  = GetUser(r.reporter_id);
                    mapped.Moderator = GetUser(r.moderator_id);
                    dto = mapped;
                    break;
                }
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Report retrieved successfully.",
                Data = dto
            };
        }
    }
}