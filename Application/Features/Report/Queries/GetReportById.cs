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
        private const string NOT_FOUND_PLACEHOLDER = "[Đối tượng không tìm thấy hoặc đã bị xóa]";

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
                return new ApiResponse { Success = false, Message = "Không tìm thấy báo cáo." };

            // preload basic users
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
                        Id = u.id,
                        Username = u.username,
                        DisplayName = u.displayname,
                        AvatarUrl = u.avata_url
                    };
                }
            }

            BaseReportResponse.UserResponse? GetUser(string? id)
                => !string.IsNullOrWhiteSpace(id) && userMap.TryGetValue(id!, out var v) ? v : null;

            var warnings = new ConcurrentBag<string>(); 
            bool isTargetDisappear = false;          

            string? novelTitle = null;
            string? chapterTitle = null;
            BaseReportResponse.UserResponse? commentAuthor = null;
            BaseReportResponse.UserResponse? forumPostAuthor = null;
            BaseReportResponse.UserResponse? forumCommentAuthor = null;

            var tasks = new List<Task>();

            // Novel 
            if (r.scope is ReportScope.Novel or ReportScope.Chapter or ReportScope.Comment)
            {
                if (!string.IsNullOrWhiteSpace(r.novel_id))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var n = await _novelRepository.GetByNovelIdAsync(r.novel_id!);
                        if (n == null)
                        {
                            novelTitle = NOT_FOUND_PLACEHOLDER;
                            if (r.scope == ReportScope.Novel) // novel là target chính
                            {
                                isTargetDisappear = true;
                                warnings.Add("Novel không tìm thấy hoặc đã bị xóa.");
                            }
                        }
                        else
                        {
                            novelTitle = n.title ?? "";
                        }
                    }, ct));
                }
            }

            if (r.scope is ReportScope.Chapter or ReportScope.Comment)
            {
                if (!string.IsNullOrWhiteSpace(r.chapter_id))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var ch = await _chapterRepository.GetByIdAsync(r.chapter_id!);
                        if (ch == null)
                        {
                            chapterTitle = NOT_FOUND_PLACEHOLDER;
                            if (r.scope == ReportScope.Chapter)
                            {
                                isTargetDisappear = true;
                                warnings.Add("Chapter không tìm thấy hoặc đã bị xóa.");
                            }
                        }
                        else
                        {
                            chapterTitle = ch.title ?? "";
                        }
                    }, ct));
                }
            }

            // Comment 
            if (r.scope == ReportScope.Comment && !string.IsNullOrWhiteSpace(r.comment_id))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var c = await _commentRepository.GetByIdAsync(r.comment_id!);
                    if (c == null)
                    {
                        isTargetDisappear = true;
                        warnings.Add("Comment không tìm thấy hoặc đã bị xóa.");
                        return;
                    }

                    var cid = c.user_id;
                    if (!string.IsNullOrWhiteSpace(cid))
                    {
                        if (!userMap.ContainsKey(cid!))
                        {
                            var u = await _userRepository.GetById(cid!);
                            if (u != null)
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
                        commentAuthor = GetUser(cid);
                    }
                }, ct));
            }

            // ForumPost 
            if (r.scope == ReportScope.ForumPost && !string.IsNullOrWhiteSpace(r.forum_post_id))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var p = await _forumPostRepository.GetByIdAsync(r.forum_post_id!);
                    if (p == null)
                    {
                        isTargetDisappear = true;
                        warnings.Add("Bài viết diễn đàn không tìm thấy hoặc đã bị xóa.");
                        return;
                    }

                    var uid = p.user_id;
                    if (!string.IsNullOrWhiteSpace(uid))
                    {
                        if (!userMap.ContainsKey(uid!))
                        {
                            var u = await _userRepository.GetById(uid!);
                            if (u != null)
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
                        forumPostAuthor = GetUser(uid);
                    }
                }, ct));
            }

            // ForumComment
            if (r.scope == ReportScope.ForumComment && !string.IsNullOrWhiteSpace(r.forum_comment_id))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var fc = await _forumCommentRepository.GetByIdAsync(r.forum_comment_id!);
                    if (fc == null)
                    {
                        isTargetDisappear = true;
                        warnings.Add("Bình luận diễn đàn không tìm thấy hoặc đã bị xóa.");
                        return;
                    }

                    var uid = fc.user_id;
                    if (!string.IsNullOrWhiteSpace(uid))
                    {
                        if (!userMap.ContainsKey(uid!))
                        {
                            var u = await _userRepository.GetById(uid!);
                            if (u != null)
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
                        forumCommentAuthor = GetUser(uid);
                    }
                }, ct));
            }

            // User
            if (r.scope == ReportScope.User && !string.IsNullOrWhiteSpace(r.target_user_id))
            {
                var target = await _userRepository.GetById(r.target_user_id);
                if (target == null)
                {
                    isTargetDisappear = true;
                    warnings.Add("User không tìm thấy hoặc đã bị xóa.");
                }
                else if (!userMap.ContainsKey(target.id))
                {
                    userMap[target.id] = new BaseReportResponse.UserResponse
                    {
                        Id = target.id,
                        Username = target.username,
                        DisplayName = target.displayname,
                        AvatarUrl = target.avata_url
                    };
                }
            }

            if (tasks.Count > 0) await Task.WhenAll(tasks);

            BaseReportResponse dto;
            switch (r.scope)
            {
                case ReportScope.Novel:
                    {
                        var mapped = _mapper.Map<NovelReportResponse>(r);
                        mapped.Reporter = GetUser(r.reporter_id);
                        mapped.Moderator = GetUser(r.moderator_id);
                        if (!string.IsNullOrWhiteSpace(novelTitle)) mapped.NovelTitle = novelTitle;
                        mapped.IsTargetDisappear = isTargetDisappear;
                        dto = mapped;
                        break;
                    }

                case ReportScope.Chapter:
                    {
                        var mapped = _mapper.Map<ChapterReportResponse>(r);
                        mapped.Reporter = GetUser(r.reporter_id);
                        mapped.Moderator = GetUser(r.moderator_id);
                        if (!string.IsNullOrWhiteSpace(chapterTitle)) mapped.ChapterTitle = chapterTitle;
                        if (!string.IsNullOrWhiteSpace(novelTitle)) mapped.NovelTitle = novelTitle;
                        mapped.IsTargetDisappear = isTargetDisappear;
                        dto = mapped;
                        break;
                    }

                case ReportScope.Comment:
                    {
                        var mapped = _mapper.Map<CommentReportResponse>(r);
                        mapped.Reporter = GetUser(r.reporter_id);
                        mapped.Moderator = GetUser(r.moderator_id);
                        mapped.CommentAuthor = commentAuthor;
                        mapped.IsTargetDisappear = isTargetDisappear;
                        dto = mapped;
                        break;
                    }

                case ReportScope.ForumPost:
                    {
                        var mapped = _mapper.Map<ForumPostReportResponse>(r);
                        mapped.Reporter = GetUser(r.reporter_id);
                        mapped.Moderator = GetUser(r.moderator_id);
                        mapped.ForumPostAuthor = forumPostAuthor;
                        mapped.IsTargetDisappear = isTargetDisappear;
                        dto = mapped;
                        break;
                    }

                case ReportScope.ForumComment:
                    {
                        var mapped = _mapper.Map<ForumCommentReportResponse>(r);
                        mapped.Reporter = GetUser(r.reporter_id);
                        mapped.Moderator = GetUser(r.moderator_id);
                        mapped.ForumCommentAuthor = forumCommentAuthor;
                        mapped.IsTargetDisappear = isTargetDisappear;
                        dto = mapped;
                        break;
                    }

                case ReportScope.User:
                    {
                        var mapped = _mapper.Map<UserReportResponse>(r);
                        mapped.Reporter = GetUser(r.reporter_id)!;
                        mapped.Moderator = GetUser(r.moderator_id);
                        mapped.TargetUser = GetUser(r.target_user_id);
                        mapped.IsTargetDisappear = isTargetDisappear;
                        dto = mapped;
                        break;
                    }

                default:
                    {
                        var mapped = _mapper.Map<BaseReportResponse>(r);
                        mapped.Reporter = GetUser(r.reporter_id);
                        mapped.Moderator = GetUser(r.moderator_id);
                        mapped.IsTargetDisappear = isTargetDisappear;
                        dto = mapped;
                        break;
                    }
            }

            var baseMsg = "Lấy báo cáo thành công.";
            var finalMsg = warnings.Count > 0
                ? $"{baseMsg} " + string.Join(" ", warnings)
                : baseMsg;

            return new ApiResponse
            {
                Success = true,
                Message = finalMsg,
                Data = dto
            };
        }
    }
}
