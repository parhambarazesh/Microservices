using System.Text;
using CommandsService.EventProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;

        // This service is going to run separately and continuously to listen to the message bus for events.
        // Then it will be a background service and developed slightly differently than the other services.
        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;
            InitializeRabbitMQ();
        }
        private void InitializeRabbitMQ()
        {
            var factory=new ConnectionFactory() {HostName=_configuration["RabbitMQHost"],
             Port=int.Parse(_configuration["RabbitMQPort"])};
            _connection=factory.CreateConnection();
            _channel=_connection.CreateModel();
            _channel.ExchangeDeclare(exchange:"trigger", type:ExchangeType.Fanout);
            _queueName=_channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue:_queueName, exchange:"trigger", routingKey:"");
            System.Console.WriteLine("--> Listening to message bus...");
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }
        // Implement the abstract to create this method.
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer=new EventingBasicConsumer(_channel);
            consumer.Received += (ModelHandler, ea) =>
            {
                System.Console.WriteLine("--> Event Received!");
                var body=ea.Body;
                var notificationMessage=Encoding.UTF8.GetString(body.ToArray());
                _eventProcessor.ProcessEvent(notificationMessage);
            };
            _channel.BasicConsume(queue:_queueName, autoAck:true, consumer:consumer);
            return Task.CompletedTask;
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            System.Console.WriteLine("--> RabbitMQ connection shutdown...");
        }
        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}