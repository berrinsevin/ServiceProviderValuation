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
using Microsoft.Extensions.Hosting;

namespace NotificationService.Tests
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

            _worker = new NotificationWorker(
                _mockRabbitMqConnectionFactory.Object, 
                _mockNotificationService.Object);
            _mockHost = new Mock<IHost>();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldProcessMessagesSuccessfully()
        {
            // Arrange
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

            // Act
            await _worker.StartAsync(CancellationToken.None);
            await Task.Delay(500);
            _mockRabbitMqChannel.Raise(channel => channel.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicConsumer>()), eventArgs);

            // Assert
            _mockNotificationService.Verify(service => 
                service.AddNotificationAsync(It.Is<NotificationDto>(n => 
                    n.RatingValue == ratingMessage.RatingValue &&
                    n.ServiceProviderId == ratingMessage.ServiceProviderId &&
                    n.UserId == ratingMessage.UserId)), 
                Times.Once);
            _mockRabbitMqChannel.Verify(channel => channel.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleDeserializationFailureGracefully()
        {
            // Arrange
            var invalidMessage = "invalid message";
            var body = Encoding.UTF8.GetBytes(invalidMessage);
            var eventArgs = new BasicDeliverEventArgs
            {
                Body = body,
                DeliveryTag = 1
            };

            _mockRabbitMqChannel.Setup(channel => channel.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            // Act
            await _worker.StartAsync(CancellationToken.None);
            _mockRabbitMqChannel.Raise(channel => channel.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicConsumer>()), eventArgs);

            // Assert
            _mockNotificationService.Verify(service => service.AddNotificationAsync(It.IsAny<NotificationDto>()), Times.Never);
            _mockRabbitMqChannel.Verify(channel => channel.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleServiceExceptionGracefully()
        {
            // Arrange
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

            _mockNotificationService
                .Setup(service => service.AddNotificationAsync(It.IsAny<NotificationDto>()))
                .ThrowsAsync(new Exception("Service error"));

            _mockRabbitMqChannel.Setup(channel => channel.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            // Act
            await _worker.StartAsync(CancellationToken.None);
            _mockRabbitMqChannel.Raise(channel => channel.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicConsumer>()), eventArgs);

            // Assert
            _mockRabbitMqChannel.Verify(channel => channel.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task StopAsync_ShouldStopWorkerGracefully()
        {
            // Arrange
            await _worker.StartAsync(CancellationToken.None);

            // Act
            await _worker.StopAsync(CancellationToken.None);

            // Assert
            _mockRabbitMqChannel.Verify(channel => channel.Close(), Times.Once);
            _mockRabbitMqChannel.Verify(channel => channel.Dispose(), Times.Once);
        }
    }
}
