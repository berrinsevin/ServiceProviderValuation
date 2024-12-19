using Microsoft.EntityFrameworkCore;
using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ServiceProviderRatingDbContext _context;

        public NotificationRepository(ServiceProviderRatingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetNewNotificationsAsync(DateTime lastFetchTime)
        {
            return await _context.Notifications
                .Where(n => n.CreatedDate > lastFetchTime)
                .ToListAsync();
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNotificationStatusAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
