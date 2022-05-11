using AutoMapper;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.Profiles
{
    public class CommandsProfile: Profile
    {
        public CommandsProfile()
        {
            // Source -> Target
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<CommandCreateDto, Command>();
            CreateMap<Command, CommandReadDto>();
            // Here we map id from the PlatformPublishedDto model to the ExternalID in the Platform.
            CreateMap<PlatformPublishedDto, Platform>()
            .ForMember(dest => dest.ExternalID, opt => opt.MapFrom(src => src.Id));
        }
    }
}