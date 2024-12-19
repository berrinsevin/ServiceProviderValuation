using Moq;
using Xunit;
using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using RabbitMQ.Client.Events;
using NotificationService.Workers;
using NotificationService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;
using NotificationService.Infrastructure.Messaging;

namespace NotificationServiceNotificationService.Tests
{
    public class NotificationWorkerTests
    {
        private readonly Mock<RabbitMqConnectionFactory> _mockRabbitMqConnectionFactory;
        private readonly Mock<IRateNotificationService> _mockNotificationService;
        private readonly Mock<IModel> _mockRabbitMqChannel;
        private readonly NotificationWorker _worker;
        private readonly Mock<IHost> _mockHost;

        public NotificationWorkerTests()
        {
            _mockRabbitMqConnectionFactory = new Mock<RabbitMqConnectionFactory>();
            _mockNotificationService = new Mock<IRateNotificationService>();
            _mockRabbitMqChannel = new Mock<IModel>();

            _mockRabbitMqConnectionFactory.Setup(factory => factory.GetChannel()).Returns(_mockRabbitMqChannel.Object);

            _worker = new NotificationWorker(_mockRabbitMqConnectionFactory.Object, _mockNotificationService.Object);
            _mockHost = new Mock<IHost>();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldProcessMessagesSuccessfully()
        {
            var ratingMessage = new RatingMessage
            {
                RatingValue = 5,
                ServiceProviderId = 123,
                UserId = 456
            };
            var message = JsonSerializer.Serialize(ratingMessage);
            var body = Encoding.UTF8.GetBytes(message);
            var eventArgs = new BasicDeliverEventArgs
            {
                Body = body,
                DeliveryTag = 1
            };

            _mockRabbitMqChannel.Setup(channel => channel.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            await _worker.StartAsync(CancellationToken.None);

            await Task.Delay(500);
            _mockRabbitMqChannel.Raise(channel => channel.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicConsumer>()), eventArgs);

            _mockNotificationService.Verify(service => service.AddNotificationAsync(It.IsAny<NotificationDto>()), Times.Once);
            _mockRabbitMqChannel.Verify(channel => channel.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleDeserializationFailureGracefully()
        {
            var invalidMessage = "invalid message"; // Invalid message format
            var body = Encoding.UTF8.GetBytes(invalidMessage);
            var eventArgs = new BasicDeliverEventArgs
            {
                Body = body,
                DeliveryTag = 1
            };

            _mockRabbitMqChannel.Setup(channel => channel.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            await _worker.StartAsync(CancellationToken.None);

            _mockRabbitMqChannel.Raise(channel => channel.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicConsumer>()), eventArgs);

            _mockNotificationService.Verify(service => service.AddNotificationAsync(It.IsAny<NotificationDto>()), Times.Never);
            _mockRabbitMqChannel.Verify(channel => channel.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
