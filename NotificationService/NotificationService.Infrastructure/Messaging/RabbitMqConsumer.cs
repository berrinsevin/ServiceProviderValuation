using RabbitMQ.Client;

namespace NotificationService.Infrastructure.Messaging
{
    public class RabbitMqConnectionFactory
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqConnectionFactory(string hostName, string userName, string password)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public IModel GetChannel() => _channel;

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}