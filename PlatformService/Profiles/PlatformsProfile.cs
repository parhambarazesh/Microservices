using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Profiles
{
    public class PlatformsProfile : Profile
    {
        //Source-->Target
        //e.g. Platform model is the source object and we want to map it to the platformDto and give the consumer the details so the
        //consumer can use the platformDto to create a platform

        //Source is our platform model and target is the platformDto
        //because our platform class and out platform dto classes have the same properties, that's the way we map them. So we don't need to
        //tell AutoMapper anything else. but in other cases we need to tell AutoMapper what to map.
        public PlatformsProfile()
        {
            //CreateMap will tell AutoMapper to map the source object to the target object. Automapper automatically figures out the properties.
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<PlatformCreateDto, Platform>();
        }
    }
}
