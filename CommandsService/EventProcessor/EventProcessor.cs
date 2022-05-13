using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.EventProcessor
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopedFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopedFactory, AutoMapper.IMapper mapper)
        {
            _scopedFactory=scopedFactory;
            _mapper=mapper;
        }
    public void ProcessEvent(string message)
    {
        var eventType=DetermineEvent(message);
        switch (eventType)
        {
            case EventType.PlatformPublished:
                addPlatform(message);
                break;
            default:
                break;
        }
    }

    private EventType DetermineEvent(string notificationMessage)
    {
        System.Console.WriteLine("--> Determining Event");
        var eventType=JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
        switch (eventType.Event)
        {
            case "Platform_Published":
                System.Console.WriteLine("Platform Published Event Detected");
                return EventType.PlatformPublished;
            default:
                System.Console.WriteLine("--> Could not determine the event type");
                return EventType.Undetermined;
        }
    }
    public void addPlatform(string platformPublishedMessage)
    {
        using(var scope=_scopedFactory.CreateScope())
        {
            var repo=scope.ServiceProvider.GetRequiredService<ICommandRepo>();
            var platformPublishedDto=JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);
            try
            {
                var plat=_mapper.Map<Platform>(platformPublishedDto);
                if(!repo.ExternalPlatformExists(plat.ExternalID))
                {
                    repo.CreatePlatform(plat);
                    repo.SaveChanges();
                    System.Console.WriteLine("Platform Added!");
                }
                else
                {
                    System.Console.WriteLine("--> Platform already exists...");
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"--> could not add Platform to DB: {ex.Message}");
            }
        }
    }
    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}
}