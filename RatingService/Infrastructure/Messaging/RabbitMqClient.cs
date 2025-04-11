using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RatingService.Infrastructure.Eventbus;
using RatingService.Infrastructure.EventBus.Events;

namespace RatingService.Infrastructure.EventBus
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

        public void PublishRateCreatedEvent(RateCreatedEvent evt)
        {
            using var channel = _connection.CreateModel();

            // Exchange tanımı
            channel.ExchangeDeclare(exchange: "ratings", type: ExchangeType.Fanout);

            var message = JsonSerializer.Serialize(evt);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: "ratings",
                routingKey: "",
                basicProperties: null,
                body: body);
        }

    }
}
