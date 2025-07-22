using AutoMapper;
using Domain.Entities;
using Shared.Contracts.Response.Comment;

namespace Application.Mapping
{
    public class CommentMap : Profile
    {
        public CommentMap()
        {
            CreateMap<CommentEntity, BaseCommentResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.content))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.Author, opt => opt.Ignore());
            CreateMap<CommentEntity, CommentResponse>()
                .IncludeBase<CommentEntity, BaseCommentResponse>()
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.like_count))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updated_at))
                .ForMember(dest => dest.ReplyCount, opt => opt.Ignore());
            CreateMap<CommentEntity, CommentReplyResponse>()
                .IncludeBase<CommentEntity, BaseCommentResponse>()
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.like_count))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updated_at))
                .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.parent_comment_id));
            CreateMap<CommentEntity, CreateCommentResponse>()
                .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.parent_comment_id))
                .IncludeBase<CommentEntity, BaseCommentResponse>();
            CreateMap<CommentEntity, UpdateCommentResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.content))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updated_at));
        }
    }
}