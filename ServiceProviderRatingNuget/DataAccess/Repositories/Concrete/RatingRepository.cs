using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess.Repositories
{
    public class RatingRepository : Repository<Rating>, IRatingRepository
    {
        private readonly ServiceProviderRatingDbContext _context;

        public RatingRepository(ServiceProviderRatingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddRatingAsync(Rating rating)
        {
            await AddAsync(rating);
            await _context.SaveChangesAsync();
        }
    }
}