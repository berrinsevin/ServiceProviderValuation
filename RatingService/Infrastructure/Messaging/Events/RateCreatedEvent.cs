namespace RatingService.Infrastructure.EventBus.Events
{
    public class RateCreatedEvent
    {
        public int ProviderId { get; set; }
        public int RatingValue { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
