using AutoMapper;
using Domain.Entities;
using Shared.Contracts.Response.Badge;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Comment;
using Shared.Contracts.Response.Follow;
using Shared.Contracts.Response.Forum;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.ReadingProcess;
using Shared.Contracts.Response.Report;
using Shared.Contracts.Response.Tag;
using Shared.Contracts.Response.Transaction;
using Shared.Contracts.Response.User;

namespace Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //User
            CreateMap<UserEntity, UserResponse>();
            CreateMap<UserEntity, UpdateUserProfileReponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.AvataUrl, opt => opt.MapFrom(src => src.avata_url))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.displayname))
                .ForMember(dest => dest.BadgeId, opt => opt.MapFrom(src => src.badge_id));
            //Novel
            CreateMap<NovelEntity, NovelResponse>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.author_id))
                .ForMember(dest => dest.NovelImage, opt => opt.MapFrom(src => src.novel_image))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.is_public))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.is_paid))
                .ForMember(dest => dest.Tags, opt => opt.Ignore());
            //CreateMap<NovelEntity, CreateNovelResponse>()
            //    .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.author_id));
            CreateMap<NovelEntity, UpdateNovelResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.NovelImage, opt => opt.MapFrom(src => src.novel_image)) // đây là string ✔️
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.tags))              // là List<string>
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.is_public))
                .ForMember(dest => dest.IsLock, opt => opt.MapFrom(src => src.is_lock))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.is_paid))
                .ForMember(dest => dest.PurchaseType, opt => opt.MapFrom(src => src.purchase_type))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.price));
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
                .ForMember(dest => dest.ScheduledAt, opt => opt.MapFrom(src => src.scheduled_at))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.is_paid))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.price))
                .ForMember(dest => dest.IsDraft, opt => opt.MapFrom(src => src.is_draft))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.is_public));

            //Tag
            CreateMap<TagEntity, TagResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name));
            CreateMap<TagEntity, UpdateTagResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name));

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

            //Badge
            CreateMap<BadgeEntity, BadgeResponse>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.icon_url))
                .ForMember(dest => dest.TriggerType, opt => opt.MapFrom(src => src.trigger_type))
                .ForMember(dest => dest.TargetAction, opt => opt.MapFrom(src => src.target_action))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at));

            //Report
            CreateMap<ReportEntity, ReportResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.user_id))
                .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.member_id))
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id))
                .ForMember(dest => dest.CommentId, opt => opt.MapFrom(src => src.comment_id))
                .ForMember(dest => dest.ForumPostId, opt => opt.MapFrom(src => src.forum_post_id))
                .ForMember(dest => dest.ForumCommentId, opt => opt.MapFrom(src => src.forum_comment_id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.reason))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updated_at));

            CreateMap<ReportEntity, UpdateReportResponse>()
                .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status));

            //Reading Process
            CreateMap<ReadingProcessEntity, ReadingProcessResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.user_id))
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updated_at));

            CreateMap<ReadingProcessEntity, UpdateReadingProcessResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.user_id))
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id));

            //Novel Follower
            CreateMap<NovelFollowerEntity, NovelFollowResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.user_id))
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.FollowedAt, opt => opt.MapFrom(src => src.followed_at));
                
            //Transaction
            CreateMap<TransactionEntity, TransactionResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.user_id))
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.payment_method))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.updated_at))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.completed_at));
            CreateMap<TransactionEntity, UserTransactionResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.payment_method))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.completed_at));
        }
    }
}
