using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Routing.Constraints;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Comment;
using Shared.Contracts.Response.Forum;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Ownership;
using Shared.Contracts.Response.Tag;
using Shared.Contracts.Response.User;

namespace Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //User
            CreateMap<UserEntity, UserResponse>();
            CreateMap<UserEntity, UpdateUserReponse>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.displayname))
                .ForMember(dest => dest.BadgeId, opt => opt.MapFrom(src => src.badge_id));
            //Novel
            CreateMap<NovelEntity, NovelResponse>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.author_id))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.is_public))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.is_paid))
                .ForMember(dest => dest.Tags, opt => opt.Ignore());
            //CreateMap<NovelEntity, CreateNovelResponse>()
            //    .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.author_id));
            CreateMap<NovelEntity, UpdateNovelResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.id));
            //Chapter
            CreateMap<ChapterEntity, ChapterResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterNumber, opt => opt.MapFrom(src => src.chapter_number))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.is_paid))
                .ForMember(dest => dest.IsDraft, opt => opt.MapFrom(src => src.is_draft))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.is_public));
                
            //CreateMap<ChapterEntity, CreateChapterResponse>()
            //    .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id));
            CreateMap<ChapterEntity, UpdateChapterResponse>()
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.ChapterNumber, opt => opt.MapFrom(src => src.chapter_number))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.is_paid))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.price));

            //Tag
            CreateMap<TagEntity, TagResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name));
            CreateMap<TagEntity, UpdateTagResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name));
            //Purchase
            CreateMap<PurchaserEntity, PurchaserResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.user_id))
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id));

            //Forum
            CreateMap<ForumPostEntity, PostResponse>()
                .ForMember(dest => dest.ImgUrls, opt => opt.MapFrom(src => src.img_urls))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.like_count))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.comment_count))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.Author, opt => opt.Ignore());
			CreateMap<ForumCommentEntity, PostCommentResponse>()
				.ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.post_id))
				.ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.parent_comment_id))
				.ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.like_count))
				.ForMember(dest => dest.ReplyCount, opt => opt.MapFrom(src => src.reply_count))
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
				.ForMember(dest => dest.Author, opt => opt.Ignore());

			//Comment
			CreateMap<CommentEntity, CommentResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.user_id))
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.content))
                .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.parent_comment_id))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.like_count))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updated_at))
                .ForMember(dest => dest.Replies, opt => opt.Ignore()) 
                .ForMember(dest => dest.UserName, opt => opt.Ignore());

            CreateMap<CommentEntity, UpdateCommentResponse>()
                .ForMember(dest => dest.CommentId, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.content));
        }
    }
}
