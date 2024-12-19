namespace ServiceProviderRatingNuget.Domain.Entities
{
    public class NotificationDto
    {
        public int ServiceProviderId { get; set; }
        public int UserId { get; set; }
        public int RatingValue { get; set; }
    }
}