using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Routing.Constraints;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;

namespace Application.Mapping
{
    public class MappingProfile: Profile    
    {
        public MappingProfile()
        {
            CreateMap<UserEntity, UserResponse>();
            //Novel
            CreateMap<NovelEntity, NovelResponse>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.author_id))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.is_public))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.is_paid)); 
            //CreateMap<NovelEntity, CreateNovelResponse>()
            //    .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.author_id));
            CreateMap<NovelEntity, UpdateNovelResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.id));
            //Chapter
            CreateMap<ChapterEntity, ChapterResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterNumber, opt => opt.MapFrom(src => src.chapter_number))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.is_paid));
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
        }
    }
}
