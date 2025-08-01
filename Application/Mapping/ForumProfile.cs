using AutoMapper;
using Domain.Entities;
using Shared.Contracts.Response.Forum;

namespace Application.Mapping
{
    public class ForumProfile : Profile
    {
        public ForumProfile()
        {
            //Post
            CreateMap<ForumPostEntity, BasePostResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.content))
                .ForMember(dest => dest.ImgUrls, opt => opt.MapFrom(src => src.img_urls))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.Author, opt => opt.Ignore());
            CreateMap<ForumPostEntity, PostResponse>()
                .IncludeBase<ForumPostEntity, BasePostResponse>()
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.like_count))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.comment_count))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updated_at));
            CreateMap<ForumPostEntity, PostCreatedResponse>()
                .IncludeBase<ForumPostEntity, BasePostResponse>();

            //PostComment
            CreateMap<ForumCommentEntity, PostCommentResponse>()
                .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.post_id))
                .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.parent_comment_id))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.like_count))
                .ForMember(dest => dest.ReplyCount, opt => opt.MapFrom(src => src.reply_count))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.Author, opt => opt.Ignore());
            CreateMap<ForumCommentEntity, CreatePostCommentResponse>()
                .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.post_id))
                .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.parent_comment_id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.user_id))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at));
        }
    }
}