using Microsoft.EntityFrameworkCore;
using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess.Repositories
{
    public class ServiceProviderRepository : IServiceProviderRepository
    {
        private readonly ServiceProviderRatingDbContext _context;

        public ServiceProviderRepository(ServiceProviderRatingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Provider>> GetProvidersAsync()
        {
            return await _context.ServiceProviders.ToListAsync();
        }

        public async Task<Provider> GetProviderByIdAsync(int serviceProviderId)
        {
            return await _context.ServiceProviders.FindAsync(serviceProviderId);
        }

        public async Task<Provider> GetProviderByNameAsync(string serviceProviderName)
        {
            return await _context.ServiceProviders.FindAsync(serviceProviderName);
        }

        public async Task AddProviderAsync(Provider provider)
        {
            _context.ServiceProviders.Add(provider);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProviderAsync(Provider provider)
        {
            //Update preserving the original RowVersion value in the database 
            _context.Entry(provider).Property("RowVersion").OriginalValue = provider.RowVersion;
            _context.ServiceProviders.Update(provider);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProviderAsync(int serviceProviderId)
        {
            var provider = await _context.ServiceProviders.FindAsync(serviceProviderId);
            if (provider != null)
            {
                _context.ServiceProviders.Remove(provider);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsProviderWithSameNameExistsAsync(string providerName)
        {
            return await _context.ServiceProviders.AnyAsync(p => p.Name == providerName);
        }
    }
}
