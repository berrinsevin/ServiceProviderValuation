using System.Text;
using RabbitMQ.Client;

namespace RatingService.Infrastructure.Messaging
{
    public class RabbitMqClient
    {
        private readonly string _hostname;
        private readonly string _queueName;
        private RabbitMQ.Client.IConnection _connection;

        public RabbitMqClient(RabbitMqSettings settings)
        {
            _hostname = settings.Hostname;
            _queueName = settings.QueueName;
            CreateConnection();
        }

        private void CreateConnection()
        {
            var factory = new ConnectionFactory { HostName = _hostname };
            _connection = factory.CreateConnection();
        }

        public void SendMessage(string message)
        {
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: String.Empty   , routingKey: _queueName, basicProperties: null, body: body);
        }
    }
}
