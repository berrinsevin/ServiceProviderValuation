using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Controller;
using NotificationService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;

namespace NotificationService.Tests
{
    public class NotificationControllerTests
    {
        private readonly Mock<IRateNotificationService> _mockNotificationService;
        private readonly NotificationController _controller;

        public NotificationControllerTests()
        {
            _mockNotificationService = new Mock<IRateNotificationService>();
            _controller = new NotificationController(_mockNotificationService.Object);
        }

        [Fact]
        public async Task GetNewNotifications_ReturnsOkResult_WithNotifications()
        {
            var userId = 1;
            var notifications = new List<Notification>
            {
                new Notification { RatingValue = 5, ServiceProviderId = 1, UserId = userId },
                new Notification { RatingValue = 4, ServiceProviderId = 2, UserId = userId }
            };

            _mockNotificationService
                .Setup(service => service.GetNewNotificationsAsync(userId))
                .Returns(Task.FromResult((IEnumerable<Notification>)notifications));

            var result = await _controller.GetNewNotifications(userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<NotificationDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetNewNotifications_ReturnsNotFound_WhenNoNotificationsFound()
        {
            var userId = 1;
            _mockNotificationService
                .Setup(service => service.GetNewNotificationsAsync(userId))
                .Returns(Task.FromResult<IEnumerable<Notification>>(new List<Notification>
                {
                    new Notification { RatingValue = 5, ServiceProviderId = 1, UserId = userId },
                    new Notification { RatingValue = 4, ServiceProviderId = 2, UserId = userId }
                }));

            var result = await _controller.GetNewNotifications(userId);

            var notFoundResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<NotificationDto>>(notFoundResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task AddNotification_ReturnsOkResult_WhenNotificationIsAdded()
        {
            var notificationDto = new NotificationDto
            {
                RatingValue = 5,
                ServiceProviderId = 1,
                UserId = 1
            };

            var result = await _controller.AddNotification(notificationDto);

            var okResult = Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task AddNotification_ReturnsBadRequest_WhenNotificationIsNull()
        {
            var result = await _controller.AddNotification(null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Notification cannot be null.", badRequestResult.Value);
        }
    }
}
