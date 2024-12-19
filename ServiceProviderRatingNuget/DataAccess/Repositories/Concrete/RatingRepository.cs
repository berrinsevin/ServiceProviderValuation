using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly ServiceProviderRatingDbContext _context;

        public RatingRepository(ServiceProviderRatingDbContext context)
        {
            _context = context;
        }

        public async Task AddRatingAsync(Rating rating) 
        { 
            _context.Ratings.Add(rating); 
            await _context.SaveChangesAsync(); 
        }
    }
}