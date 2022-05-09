using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;
using System;
using System.Collections.Generic;

namespace PlatformService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        // Here we inject IPlatformRepo, IMapper and ICommandDataClient into the constructor
        // Because we have added the AddHttpClient to the Program.cs, we can use the HttpClient here.
        // This is how dependency injection works in ASP.NET Core. but make sure you have registered them
        // in the Program.cs
        public PlatformsController(
            IPlatformRepo repository,
            IMapper mapper,
            ICommandDataClient commandDataClient,
            IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }

        //When you call this, we return an enumeartion of our platform read dto, not platform entity, not platform model
        //When we send data out, we want to send over dto.
        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            System.Console.WriteLine("-->Getting Platforms.....");
            var platformItems = _repository.GetAllPlatforms();

            //Here we use CreateMap<Platform, PlatformDto>(); line in PlatformProfile.cs
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var platformItem = _repository.GetPlatformById(id);
            if (platformItem != null)
            {
                //platformItem is the source. PlatformReadDto is the output.
                return Ok(_mapper.Map<PlatformReadDto>(platformItem));
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            //Mapping from platformCreateDto to platform
            var platformModel = _mapper.Map<Platform>(platformCreateDto);
            _repository.CreatePlatform(platformModel);
            _repository.SaveChanges();
            //Mapping from platform to platformReadDto
            var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);
            //Send the platformReadDto to the Command Service (sync message)
            try
            {
                await _commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch (Exception ex)
            {
                // If the command service is down, we will not get a synchronous response. but still we can return the platformReadDto
                System.Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
            }

            // Send the platformReadDto to the Message Bus (async message)
            try
            {
                var platformPublishedDto=_mapper.Map<PlatformPublishedDto>(platformReadDto);
                // we dont have Event properties in PlatformPublishedDto, so we should define it explicitelly here.
                platformPublishedDto.Event = "PlatformCreated";
                _messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
            }
            //Return HTTP with the created item
            return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
        }
    }
}
