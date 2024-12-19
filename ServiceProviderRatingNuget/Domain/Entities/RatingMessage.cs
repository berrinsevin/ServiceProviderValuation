namespace ServiceProviderRatingNuget.Domain.Entities
{
    public class RatingMessage
    {
        public int UserId { get; set; }
        public int ServiceProviderId { get; set; }
        public int RatingValue { get; set; }
    }
}