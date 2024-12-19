using ServiceProviderRatingNuget.Domain.Entities;

namespace NotificationService.Business.Services
{
    public interface IRateNotificationService
    {
        Task<IEnumerable<Notification>> GetNewNotificationsAsync(int userId);
        Task AddNotificationAsync(NotificationDto notification);
    }
}
