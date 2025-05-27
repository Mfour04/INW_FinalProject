using AutoMapper;
using Domain.Entities;
using Shared.Contracts.Response;

namespace Application.Mapping
{
    public class MappingProfile: Profile    
    {
        public MappingProfile()
        {
            CreateMap<UserEntity, UserResponse>();
            CreateMap<NovelEntity, NovelResponse>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.author_id)); ;
            CreateMap<NovelEntity, CreateNovelResponse>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.author_id));
            CreateMap<NovelEntity, UpdateNovelResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.id)); 
        }
    }
}
