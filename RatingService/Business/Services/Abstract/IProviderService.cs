using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Business.Services
{
    public interface IProviderService
    {
        Task<IEnumerable<Provider>> GetProvidersAsync();
        Task<Provider> GetProviderByIdAsync(int serviceProviderId);
        Task AddProviderAsync(ProviderDto provider);
        Task UpdateProviderAsync(Provider provider);
        Task DeleteProviderAsync(int serviceProviderId);
    }
}
