using AutoMapper;
using Entities.Dtos;
using Entities.Models;

namespace AccountControlPanel_AspNetCore6.Infrastructure.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CustomUser, UserDto>();
            CreateMap<CustomUser, UserDto>().ReverseMap();
            CreateMap<UserDtoForCreation, CustomUser>();
            CreateMap<UserDtoForUpdate, CustomUser>().ReverseMap();
        }
    }
}
