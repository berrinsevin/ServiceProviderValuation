using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess.Repositories
{
    public interface IRatingRepository
    {
        Task AddRatingAsync(Rating rating);
    }
}
