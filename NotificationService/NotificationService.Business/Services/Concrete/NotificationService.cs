using Serilog;
using ServiceProviderRatingNuget.Domain.Entities;
using ServiceProviderRatingNuget.DataAccess.Repositories;

namespace NotificationService.Business.Services
{
    public class RateNotificationService : IRateNotificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;

        public RateNotificationService(IUserRepository userRepository, INotificationRepository notificationRepository)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Retrieves new notifications that were created after the specified date and are not marked as sent.
        /// </summary>
        /// <param name="lastFetchTime">The last time the notifications were fetched.</param>
        /// <returns>A list of new notifications.</returns>
        public async Task<IEnumerable<Notification>> GetNewNotificationsAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found.");
                }

                /// Long-term, fetching the last fetch time from the database on each request can lead to performance issues
                /// A more efficient approach would be to store and retrieve this information from Redis, which can provide faster access and reduce the load on the database.
                var lastFetchTime = user.LastFetchTime ?? DateTime.MinValue;

                var notifications = await _notificationRepository.GetNewNotificationsAsync(lastFetchTime);
                Log.Information("Fetched {Count} new notifications since {LastFetchTime}.", notifications.Count(), lastFetchTime);
                return notifications;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving new notifications for user: {userId}.", userId);
                throw;
            }
        }

        /// <summary>
        /// Adds a new notification.
        /// </summary>
        /// <param name="notification">The notification to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddNotificationAsync(NotificationDto notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification), "Notification cannot be null.");
            }

            try
            {
                await _notificationRepository.AddNotificationAsync(new()
                {
                    UserId = notification.UserId,
                    RatingValue = notification.RatingValue,
                    ServiceProviderId = notification.ServiceProviderId,
                    CreatedDate = DateTime.Now
                });

                Log.Information("Added new notification to : {ProviderId}", notification.ServiceProviderId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding a new notification.");
                throw;
            }
        }
    }
}
