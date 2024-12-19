namespace ServiceProviderRatingNuget.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? LastFetchTime { get; set; }
        public ICollection<Rating> Ratings { get; set; }
    }
}
