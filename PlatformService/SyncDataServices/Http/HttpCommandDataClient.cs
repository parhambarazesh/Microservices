using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using Microsoft.Extensions.Configuration;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpCommandDataClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task SendPlatformToCommand(PlatformReadDto plat)
        {
            var httpClient=new StringContent(
                JsonSerializer.Serialize(plat),
                Encoding.UTF8,
                "application/json"
            );
            var response=await _httpClient.PostAsync($"{_configuration["CommandService"]}", httpClient);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"--> Sync POST to Command Service was OK!");
            }
            else
            {
                Console.WriteLine($"--> Sync POST to Command Service was NOT OK!");
            }
        }
    }
}