using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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

        // Here we inject IPlatformRepo, IMapper and ICommandDataClient into the constructor
        // Because we have added the AddHttpClient to the Program.cs, we can use the HttpClient here.
        public PlatformsController(
            IPlatformRepo repository,
            IMapper mapper,
            ICommandDataClient commandDataClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
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
        public async Task<ActionResult<PlatformReadDto>> CreatePlatformAsync(PlatformCreateDto platformCreateDto)
        {
            //Mapping from platformCreateDto to platform
            var platformModel = _mapper.Map<Platform>(platformCreateDto);
            _repository.CreatePlatform(platformModel);
            _repository.SaveChanges();
            //Mapping from platform to platformReadDto
            var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);
            //Send the platformReadDto to the Command Service
            try
            {
                await _commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
                
            }
            //Return HTTP with the created item
            return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
        }
    }
}
