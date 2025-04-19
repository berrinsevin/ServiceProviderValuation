using Moq;
using Xunit;
using NotificationService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;
using ServiceProviderRatingNuget.DataAccess.Repositories;

namespace NotificationService.NotificationServiceTests
{
    public class RateNotificationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<INotificationRepository> _mockNotificationRepository;
        private readonly RateNotificationService _rateNotificationService;

        public RateNotificationServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockNotificationRepository = new Mock<INotificationRepository>();
            _rateNotificationService = new RateNotificationService(
                _mockUserRepository.Object, 
                _mockNotificationRepository.Object);
        }

        [Fact]
        public async Task GetNewNotificationsAsync_UserNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            int userId = 1;
            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _rateNotificationService.GetNewNotificationsAsync(userId));
            Assert.Equal("User not found.", exception.Message);
        }

        [Fact]
        public async Task GetNewNotificationsAsync_ValidUser_ShouldReturnNotifications()
        {
            // Arrange
            int userId = 1;
            var user = new User { Id = userId, LastFetchTime = DateTime.Now.AddHours(-1) };
            var notifications = new List<Notification>
            {
                new Notification { Id = 1, UserId = userId, RatingValue = 5, ServiceProviderId = 1, CreatedDate = DateTime.Now }
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockNotificationRepository.Setup(repo => repo.GetNewNotificationsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(notifications);

            // Act
            var result = await _rateNotificationService.GetNewNotificationsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(5, result.First().RatingValue);
            _mockUserRepository.Verify(repo => repo.UpdateUserLastFetchTimeAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetNewNotificationsAsync_ShouldUpdateLastFetchTime()
        {
            // Arrange
            int userId = 1;
            var user = new User { Id = userId, LastFetchTime = DateTime.Now.AddHours(-1) };
            var notifications = new List<Notification>();

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockNotificationRepository.Setup(repo => repo.GetNewNotificationsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(notifications);

            // Act
            await _rateNotificationService.GetNewNotificationsAsync(userId);

            // Assert
            _mockUserRepository.Verify(repo => repo.UpdateUserLastFetchTimeAsync(userId), Times.Once);
        }

        [Fact]
        public async Task AddNotificationAsync_ValidNotification_ShouldAddNotification()
        {
            // Arrange
            var notificationDto = new NotificationDto
            {
                UserId = 1,
                RatingValue = 4,
                ServiceProviderId = 2
            };

            // Act
            await _rateNotificationService.AddNotificationAsync(notificationDto);

            // Assert
            _mockNotificationRepository.Verify(repo => 
                repo.AddNotificationAsync(It.Is<Notification>(n => 
                    n.UserId == notificationDto.UserId &&
                    n.RatingValue == notificationDto.RatingValue &&
                    n.ServiceProviderId == notificationDto.ServiceProviderId)), 
                Times.Once);
        }

        [Fact]
        public async Task AddNotificationAsync_NullNotification_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _rateNotificationService.AddNotificationAsync(null));
            Assert.Equal("Notification cannot be null. (Parameter 'notification')", exception.Message);
        }

        [Fact]
        public async Task AddNotificationAsync_InvalidRatingValue_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidNotification = new NotificationDto
            {
                UserId = 1,
                RatingValue = 6, // Invalid rating value
                ServiceProviderId = 2
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _rateNotificationService.AddNotificationAsync(invalidNotification));
            Assert.Equal("Rating value must be between 1 and 5.", exception.Message);
        }
    }
}
