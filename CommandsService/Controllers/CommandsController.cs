using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController:ControllerBase
    {
        private readonly ICommandRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            System.Console.WriteLine($"--> Hit GetCommandsForPlatform: {platformId}");
            if (!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }
            var commands = _repository.GetCommandsForPlatform(platformId);
            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
        }
        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            System.Console.WriteLine($"--> Hit GetCommandForPlatform: {platformId}/{commandId}");
            if (!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }
            var command = _repository.GetCommand(platformId, commandId);
            if (command == null)
            {
                return NotFound();
            }
            // here we are mapping the command to the dto
            return Ok(_mapper.Map<CommandReadDto>(command));
        }
        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandDto)
        {
            System.Console.WriteLine($"--> Hit CreateCommandForPlatform: {platformId}");
            if (!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }
            var command = _mapper.Map<Command>(commandDto);
            _repository.CreateCommand(platformId, command);
            _repository.SaveChanges();
            // because we have saved the changes to the database, command variable is now has a valid id.
            var commandReadDto = _mapper.Map<CommandReadDto>(command);
            return CreatedAtRoute(nameof(CreateCommandForPlatform),
             new {platformId = platformId, commandId = commandReadDto.Id}, commandReadDto);
        }
    }
}