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
            // Arrange
            var userId = 1;
            var notifications = new List<Notification>
            {
                new Notification { RatingValue = 5, ServiceProviderId = 1, UserId = userId },
                new Notification { RatingValue = 4, ServiceProviderId = 2, UserId = userId }
            };

            _mockNotificationService
                .Setup(service => service.GetNewNotificationsAsync(userId))
                .ReturnsAsync(notifications);

            // Act
            var result = await _controller.GetNewNotifications(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<NotificationDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal(5, returnValue[0].RatingValue);
            Assert.Equal(4, returnValue[1].RatingValue);
        }

        [Fact]
        public async Task GetNewNotifications_ReturnsNotFound_WhenNoNotificationsFound()
        {
            // Arrange
            var userId = 1;
            _mockNotificationService
                .Setup(service => service.GetNewNotificationsAsync(userId))
                .ReturnsAsync(new List<Notification>());

            // Act
            var result = await _controller.GetNewNotifications(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No new notifications found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetNewNotifications_ReturnsBadRequest_WhenUserIdIsInvalid()
        {
            // Arrange
            var invalidUserId = 0;

            // Act
            var result = await _controller.GetNewNotifications(invalidUserId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid user ID.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddNotification_ReturnsOkResult_WhenNotificationIsAdded()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                RatingValue = 5,
                ServiceProviderId = 1,
                UserId = 1
            };

            // Act
            var result = await _controller.AddNotification(notificationDto);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _mockNotificationService.Verify(service => service.AddNotificationAsync(notificationDto), Times.Once);
        }

        [Fact]
        public async Task AddNotification_ReturnsBadRequest_WhenNotificationIsNull()
        {
            // Act
            var result = await _controller.AddNotification(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Notification cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddNotification_ReturnsBadRequest_WhenNotificationIsInvalid()
        {
            // Arrange
            var invalidNotification = new NotificationDto
            {
                RatingValue = 6, // Invalid rating value
                ServiceProviderId = 0, // Invalid service provider ID
                UserId = 0 // Invalid user ID
            };

            // Act
            var result = await _controller.AddNotification(invalidNotification);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid notification data.", badRequestResult.Value);
        }
    }
}
