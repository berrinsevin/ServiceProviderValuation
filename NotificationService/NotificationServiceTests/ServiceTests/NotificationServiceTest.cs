using Moq;
using Xunit;
using Serilog;
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
            _rateNotificationService = new RateNotificationService(_mockUserRepository.Object, _mockNotificationRepository.Object);
        }

        [Fact]
        public async Task GetNewNotificationsAsync_UserNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            int userId = 1;
            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _rateNotificationService.GetNewNotificationsAsync(userId));
            Assert.Equal("User not found.", exception.Message);
        }

        [Fact]
        public async Task GetNewNotificationsAsync_ValidUser_ShouldReturnNotifications()
        {
            int userId = 1;
            var user = new User { Id = userId, LastFetchTime = DateTime.Now.AddHours(-1) };
            var notifications = new List<Notification>
            {
                new Notification { Id = 1, UserId = userId, RatingValue = 5, ServiceProviderId = 1, CreatedDate = DateTime.Now }
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockNotificationRepository.Setup(repo => repo.GetNewNotificationsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(notifications);

            var result = await _rateNotificationService.GetNewNotificationsAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(5, result.First().RatingValue);
        }

        [Fact]
        public async Task AddNotificationAsync_ValidNotification_ShouldAddNotification()
        {
            var notificationDto = new NotificationDto
            {
                UserId = 1,
                RatingValue = 4,
                ServiceProviderId = 2
            };

            await _rateNotificationService.AddNotificationAsync(notificationDto);

            _mockNotificationRepository.Verify(repo => repo.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }

        [Fact]
        public async Task AddNotificationAsync_NullNotification_ShouldThrowArgumentNullException()
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _rateNotificationService.AddNotificationAsync(null));
            Assert.Equal("Notification cannot be null. (Parameter 'notification')", exception.Message);
        }

        [Fact]
        public async Task GetNewNotificationsAsync_LogsInformation_WhenNotificationsFetched()
        {
            int userId = 1;
            var user = new User { Id = userId, LastFetchTime = DateTime.Now.AddHours(-1) };
            var notifications = new List<Notification>
            {
                new Notification { Id = 1, UserId = userId, RatingValue = 5, ServiceProviderId = 1, CreatedDate = DateTime.Now }
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockNotificationRepository.Setup(repo => repo.GetNewNotificationsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(notifications);

            var logger = new Mock<Serilog.ILogger>();
            Log.Logger = logger.Object;

            await _rateNotificationService.GetNewNotificationsAsync(userId);

            logger.Verify(l => l.Information(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task AddNotificationAsync_LogsInformation_WhenNotificationAdded()
        {
            var notificationDto = new NotificationDto
            {
                UserId = 1,
                RatingValue = 4,
                ServiceProviderId = 2
            };

            var logger = new Mock<Serilog.ILogger>();
            Log.Logger = logger.Object;

            await _rateNotificationService.AddNotificationAsync(notificationDto);

            logger.Verify(l => l.Information(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
