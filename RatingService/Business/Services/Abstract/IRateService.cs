using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Business.Services
{
    public interface IRateService
    {
        Task AddRatingAsync(RatingDto rating);
    }
}
