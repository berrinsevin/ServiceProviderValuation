using System.ComponentModel.DataAnnotations;

namespace ServiceProviderRatingNuget.Domain.Entities
{
    public class Provider 
    { 
        public int Id { get; set; } 
        public string Name { get; set; } 
        public double AverageRating { get; set; } 
        public int RatingCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public ICollection<Rating> Ratings { get; set; }

        // RowVersion property for concurrency control 
        [Timestamp] 
        public byte[] RowVersion { get; set; } 
    }
}
