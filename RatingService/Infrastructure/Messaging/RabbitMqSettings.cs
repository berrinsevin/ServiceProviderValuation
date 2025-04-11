namespace RatingService.Infrastructure.Eventbus
{
    public class RabbitMqSettings
    {
        public string Hostname { get; set; }
        public string QueueName { get; set; }
    }
}
