namespace NotificationService.Infrastructure
{
    public class RateCreatedEvent
    {
        public int ServiceProviderId { get; set; }
        public int RatingValue { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
