using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess.Repositories
{
    public interface IServiceProviderRepository 
    { 
        Task<IEnumerable<Provider>> GetProvidersAsync();
        Task<Provider> GetProviderByIdAsync(int serviceProviderId);
        Task<Provider> GetProviderByNameAsync(string serviceProviderName);
        Task AddProviderAsync(Provider provider);
        Task UpdateProviderAsync(Provider provider);
        Task DeleteProviderAsync(int serviceProviderId);
        Task<bool> IsProviderWithSameNameExistsAsync(string providerName);
    }
}
