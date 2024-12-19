using Serilog;
using ServiceProviderRatingNuget.Domain.Entities;
using ServiceProviderRatingNuget.DataAccess.Repositories;

namespace RatingService.Business.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IServiceProviderRepository _providerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderService"/> class.
        /// </summary>
        /// <param name="providerRepository">The provider repository.</param>
        public ProviderService(IServiceProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
        }

        /// <summary>
        /// Retrieves all providers asynchronously.
        /// </summary>
        /// <returns>A list of providers.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while retrieving providers.</exception>
        public async Task<IEnumerable<Provider>> GetProvidersAsync()
        {
            try
            {
                return await _providerRepository.GetProvidersAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving providers.");
                throw;
            }
        }

        /// <summary>
        /// Even though provider names are unique, they are fetched using the primary key (Id) column
        /// Providers will be listed by name on the front-end side but the selected provider will be queried by its Id in the database
        /// </summary>
        /// <param name="serviceProviderId">The ID of the service provider to retrieve</param>
        /// <returns>The provider with the specified ID.</returns>
        /// <exception cref="ArgumentException">Thrown when the serviceProviderId is less than or equal to zero</exception>
        /// <exception cref="Exception">Thrown when an error occurs while retrieving the provider</exception>
        public async Task<Provider> GetProviderByIdAsync(int serviceProviderId)
        {
            if (serviceProviderId <= 0)
            {
                throw new ArgumentException("Service provider ID must be greater than zero.", nameof(serviceProviderId));
            }

            try
            {
                return await _providerRepository.GetProviderByIdAsync(serviceProviderId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving provider with ID {ServiceProviderId}", serviceProviderId);
                throw;
            }
        }

        /// <summary>
        /// Adds a new provider
        /// </summary>
        /// <param name="provider">The provider to add</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provider is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when a provider with the same name already exists</exception>
        /// <exception cref="Exception">Thrown when an error occurs while adding the provider</exception>
        public async Task AddProviderAsync(ProviderDto provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider), "Provider cannot be null.");
            }

            if (string.IsNullOrEmpty(provider.Name))
            {
                throw new ArgumentNullException(nameof(provider.Name), "Provider name cannot be null.");
            }

            try
            {
                if (await _providerRepository.IsProviderWithSameNameExistsAsync(provider.Name))
                {
                    throw new InvalidOperationException($"Provider with name {provider.Name} already exists.");
                }

                await _providerRepository.AddProviderAsync(new() {
                    Name = provider.Name,
                    RatingCount = 0,
                    AverageRating = 0,
                    LastUpdatedDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding provider {ProviderName}", provider.Name);
                throw;
            }
        }

        /// <summary> 
        /// Updates a provider
        /// </summary> 
        /// <param name="provider">The provider to update</param>
        /// <returns>A task representing the asynchronous operation</returns> 
        /// <exception cref="ArgumentNullException">Thrown when the provider is null</exception> 
        /// <exception cref="ArgumentException">Thrown when the provider ID is less than or equal to zero
        /// </exception> /// <exception cref="Exception">Thrown when an error occurs while updating the provider</exception> 
        public async Task UpdateProviderAsync(Provider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider), "Provider cannot be null.");
            }

            if (provider.Id <= 0)
            {
                throw new ArgumentException("Provider ID must be greater than zero.", nameof(provider.Id));
            }

            try
            {
                await _providerRepository.UpdateProviderAsync(new() {
                    
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating provider with ID {ProviderId}", provider.Id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a provider by ID
        /// </summary>
        /// <param name="serviceProviderId">The ID of the provider to delete</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the serviceProviderId is less than or equal to zero</exception>
        /// <exception cref="Exception">Thrown when an error occurs while deleting the provider</exception>
        public async Task DeleteProviderAsync(int serviceProviderId)
        {
            if (serviceProviderId <= 0)
            {
                throw new ArgumentException("Service provider ID must be greater than zero.", nameof(serviceProviderId));
            }

            try
            {
                await _providerRepository.DeleteProviderAsync(serviceProviderId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting provider with ID {ServiceProviderId}", serviceProviderId);
                throw;
            }
        }
    }
}
