using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetNewNotificationsAsync(DateTime lastFetchTime);

        Task AddNotificationAsync(Notification notification);
    }
}
