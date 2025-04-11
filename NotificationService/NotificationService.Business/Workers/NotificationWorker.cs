using Serilog;
using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using RabbitMQ.Client.Events;
using NotificationService.Infrastructure;
using NotificationService.Business.Services;
using NotificationService.Infrastructure.EventBus;


namespace NotificationService.Workers
{
    /// <summary>
    /// Worker service to handle rating events from RabbitMQ and create notifications.
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
            _rabbitMqChannel.ExchangeDeclare("ratings", ExchangeType.Fanout);
            _rabbitMqChannel.QueueDeclare(queue: "rating_queue",
                                          durable: true,
                                          exclusive: false,
                                          autoDelete: false,
                                          arguments: null);
            _rabbitMqChannel.QueueBind("rating_queue", "ratings", "");

            var consumer = new EventingBasicConsumer(_rabbitMqChannel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Log.Information("Received message: {Message}", message);

                RateCreatedEvent? rateEvent;
                try
                {
                    rateEvent = JsonSerializer.Deserialize<RateCreatedEvent>(message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error deserializing message: {Message}", message);
                    return;
                }

                if (rateEvent == null)
                {
                    Log.Warning("Deserialized RateCreatedEvent is null. Raw message: {Message}", message);
                    return;
                }

                try
                {
                    await _notificationService.AddNotificationAsync(new()
                    {
                        RatingValue = rateEvent.RatingValue,
                        ServiceProviderId = rateEvent.ServiceProviderId,
                        UserId = rateEvent.UserId
                    });

                    _rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to process event for provider {ProviderId}", rateEvent.ServiceProviderId);
                }
            };

            _rabbitMqChannel.BasicConsume(queue: "rating_queue",
                                          autoAck: false,
                                          consumer: consumer);

            return Task.CompletedTask;
        }
    }
}