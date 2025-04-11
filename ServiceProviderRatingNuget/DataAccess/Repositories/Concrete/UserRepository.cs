using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ServiceProviderRatingDbContext _context;

        public UserRepository(ServiceProviderRatingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(int userId) => await GetByIdAsync(userId);

        public async Task AddUserAsync(User user)
        {
            await AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}

