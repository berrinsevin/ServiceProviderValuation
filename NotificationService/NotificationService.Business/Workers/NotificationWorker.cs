using Serilog;
using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using NotificationService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;
using NotificationService.Infrastructure.Messaging;

namespace NotificationService.Workers
{
    /// <summary>
    /// Worker service to handle rating messages from RabbitMQ and create notifications.
    /// </summary>
    public class NotificationWorker : BackgroundService
    {
        private readonly RabbitMqConnectionFactory _rabbitMqConnectionFactory;
        private readonly IModel _rabbitMqChannel;
        private readonly IRateNotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationWorker"/> class.
        /// </summary>
        /// <param name="rabbitMqConnectionFactory">The RabbitMQ connection factory.</param>
        /// <param name="context">The application database context.</param>
        public NotificationWorker(RabbitMqConnectionFactory rabbitMqConnectionFactory, IRateNotificationService notificationService)
        {
            _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
            _notificationService = notificationService;
            _rabbitMqChannel = _rabbitMqConnectionFactory.GetChannel();
        }

        /// <summary>
        /// Executes the worker service.
        /// </summary>
        /// <param name="stoppingToken">The token used to stop the service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbitMqChannel.QueueDeclare(queue: "rating_queue",
                                          durable: true,
                                          exclusive: false,
                                          autoDelete: false,
                                          arguments: null);

            var consumer = new EventingBasicConsumer(_rabbitMqChannel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Log.Information("Received message: {Message}", message);

                var ratingMessage = JsonSerializer.Deserialize<RatingMessage>(message);

                if (ratingMessage == null)
                {
                    throw new ArgumentException("Failed to deserialize message: {Message}", message); 
                }

                await _notificationService.AddNotificationAsync(new()
                {
                    RatingValue = ratingMessage.RatingValue,
                    ServiceProviderId = ratingMessage.ServiceProviderId,
                    UserId = ratingMessage.UserId
                });

                _rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _rabbitMqChannel.BasicConsume(queue: "rating_queue",
                                          autoAck: false,
                                          consumer: consumer);

            return Task.CompletedTask;
        }
    }
}