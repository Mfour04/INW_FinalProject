using AutoMapper;
using Domain.Entities;
using Shared.Contracts.Response.Report;

namespace Application.Mapping
{
    public class ReportProfile : Profile
    {
        public ReportProfile()
        {
            CreateMap<ReportEntity, BaseReportResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Scope, opt => opt.MapFrom(src => src.scope))
                .ForMember(dest => dest.Reporter, opt => opt.Ignore())
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.reason))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.message))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.action))
                .ForMember(dest => dest.Moderator, opt => opt.Ignore())
                .ForMember(dest => dest.ModeratorNote, opt => opt.MapFrom(src => src.moderator_note))
                .ForMember(dest => dest.ModeratedAt, opt => opt.MapFrom(src => src.moderated_at))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updated_at));
            CreateMap<ReportEntity, NovelReportResponse>()
                .IncludeBase<ReportEntity, BaseReportResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.NovelTitle, opt => opt.Ignore());
            CreateMap<ReportEntity, ChapterReportResponse>()
                .IncludeBase<ReportEntity, BaseReportResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id))
                .ForMember(dest => dest.NovelTitle, opt => opt.Ignore())
                .ForMember(dest => dest.ChapterTitle, opt => opt.Ignore());
            CreateMap<ReportEntity, CommentReportResponse>()
                .IncludeBase<ReportEntity, BaseReportResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id))
                .ForMember(dest => dest.CommentId, opt => opt.MapFrom(src => src.comment_id))
                .ForMember(dest => dest.CommentAuthor, opt => opt.Ignore());
            CreateMap<ReportEntity, ForumPostReportResponse>()
                .IncludeBase<ReportEntity, BaseReportResponse>()
                .ForMember(dest => dest.ForumPostId, opt => opt.MapFrom(src => src.forum_post_id))
                .ForMember(dest => dest.ForumPostAuthor, opt => opt.Ignore());
            CreateMap<ReportEntity, ForumCommentReportResponse>()
                .IncludeBase<ReportEntity, BaseReportResponse>()
                .ForMember(dest => dest.ForumCommentId, opt => opt.MapFrom(src => src.forum_comment_id))
                .ForMember(dest => dest.ForumCommentAuthor, opt => opt.Ignore());
            CreateMap<ReportEntity, UserReportResponse>()
                .IncludeBase<ReportEntity, BaseReportResponse>()
                .ForMember(dest => dest.TargetUserId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.TargetUser, opt => opt.Ignore());
        }
    }
}