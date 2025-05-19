using AutoMapper;
using Domain.Entities;
using Shared.Contracts.Respone;

namespace Application.Mapping
{
    public class MappingProfile: Profile    
    {
        public MappingProfile()
        {
            CreateMap<UserEntity, UserRespone>();
        }
    }
}
