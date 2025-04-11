using Microsoft.EntityFrameworkCore;
using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess.Repositories
{
    public class ServiceProviderRepository : Repository<Provider>, IServiceProviderRepository
    {
        private readonly ServiceProviderRatingDbContext _context;

        public ServiceProviderRepository(ServiceProviderRatingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Provider>> GetProvidersAsync()
            => await GetAllAsync();

        public async Task<Provider> GetProviderByIdAsync(int serviceProviderId)
            => await GetByIdAsync(serviceProviderId);

        public async Task<Provider> GetProviderByNameAsync(string serviceProviderName)
            => await _context.ServiceProviders.FirstOrDefaultAsync(p => p.Name == serviceProviderName);

        public async Task AddProviderAsync(Provider provider)
        {
            await AddAsync(provider);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProviderAsync(Provider provider)
        {
            _context.Entry(provider).Property("RowVersion").OriginalValue = provider.RowVersion;
            Update(provider);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProviderAsync(int serviceProviderId)
        {
            var provider = await GetByIdAsync(serviceProviderId);
            if (provider != null)
            {
                Remove(provider);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsProviderWithSameNameExistsAsync(string providerName)
            => await _context.ServiceProviders.AnyAsync(p => p.Name == providerName);
    }
}
