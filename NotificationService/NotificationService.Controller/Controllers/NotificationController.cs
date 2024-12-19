using Microsoft.AspNetCore.Mvc;
using NotificationService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;

namespace NotificationService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IRateNotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class
        /// </summary>
        /// <param name="notificationService">The notification service</param>
        public NotificationController(IRateNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Retrieves new notifications that were created after the specified date and are not marked as sent
        /// </summary>
        /// <param name="lastFetchTime">The last time the notifications were fetched.</param>
        /// <returns>A list of new notifications.</returns>
        [HttpGet("new")]
        public async Task<IActionResult> GetNewNotifications([FromQuery] int userId)
        {
            var notifications = await _notificationService.GetNewNotificationsAsync(userId);
            return Ok(notifications);
        }

        /// <summary>
        /// Adds a new notification.
        /// </summary>
        /// <param name="notification">The notification to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> AddNotification([FromBody] NotificationDto notification)
        {
            if (notification == null)
            {
                return BadRequest("Notification cannot be null.");
            }

            await _notificationService.AddNotificationAsync(notification);
            return Ok();
        }
    }
}
