using AutoMapper;
using Domain.Entities;
using Shared.Contracts.Respone;
using Shared.Contracts.Response;

namespace Application.Mapping
{
    public class MappingProfile: Profile    
    {
        public MappingProfile()
        {
            CreateMap<UserEntity, UserResponse>();
            CreateMap<NovelEntity, NovelResponse>();
            CreateMap<NovelEntity, UpdateNovelResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.id)); ;
        }
    }
}
