using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"],
             Port = int.Parse(_configuration["RabbitMQPort"]) };
            try
            {
                // in RabbitMQ we need to define connection and channel as well as the exchange.
                _connection = factory.CreateConnection();
                _channel=_connection.CreateModel();
                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
                System.Console.WriteLine("--> Connected to Message Bus");
            }
            catch (System.Exception ex)
            {
                 Console.WriteLine($"--> Could not connect to the Message bus: {ex.Message}");
            }
        }
        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            // convert to serialized json
            var message = JsonSerializer.Serialize(platformPublishedDto);
            if (_connection.IsOpen)
            {
                System.Console.WriteLine("--> RabbitMQ Connecton Open, Sending Message...");
                SendMessage(message);
            }
            else
            {
                System.Console.WriteLine("--> RabbitMQ Connecton Closed, Not Sending...");
            }
        }
        private void SendMessage(string message)
        {
            var body=Encoding.UTF8.GetBytes(message);
            // we don't need routing key here, because we are using fanout exchange.
            _channel.BasicPublish(exchange: "trigger",
                                  routingKey: "",
                                  basicProperties: null,
                                  body: body);
            System.Console.WriteLine($"--> We have sent {message} to the Message Bus");
        }
        public void Dispose()
        {
            System.Console.WriteLine("MessageBus Disposed");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }
        public void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
        }
    }
}