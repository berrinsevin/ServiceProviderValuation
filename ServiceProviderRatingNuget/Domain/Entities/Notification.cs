namespace ServiceProviderRatingNuget.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int ServiceProviderId { get; set; }
        public int UserId { get; set; }
        public int RatingValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public Provider Provider { get; set; }
        public User User { get; set; }
    }
}