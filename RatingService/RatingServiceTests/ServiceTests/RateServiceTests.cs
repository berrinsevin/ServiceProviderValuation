using Moq;
using RatingService.Business.Services;
using RatingService.Infrastructure.Messaging;
using ServiceProviderRatingNuget.DataAccess.Repositories;
using ServiceProviderRatingNuget.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RatingService.Tests
{
    public class RateServiceTests
    {
        private readonly Mock<IRatingRepository> _mockRatingRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IServiceProviderRepository> _mockServiceProviderRepository;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly Mock<ConnectionMultiplexer> _mockRedisConnection;
        private readonly Mock<RabbitMqClient> _mockNotificationRabbitMqClient;
        private readonly RateService _rateService;

        public RateServiceTests()
        {
            _mockRatingRepository = new Mock<IRatingRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockServiceProviderRepository = new Mock<IServiceProviderRepository>();
            _mockCache = new Mock<IDistributedCache>();
            _mockRedisConnection = new Mock<ConnectionMultiplexer>();
            _mockNotificationRabbitMqClient = new Mock<RabbitMqClient>();
            _rateService = new RateService(
                _mockRatingRepository.Object,
                _mockUserRepository.Object,
                _mockServiceProviderRepository.Object,
                _mockCache.Object,
                _mockRedisConnection.Object,
                _mockNotificationRabbitMqClient.Object);
        }

        [Fact]
        public async Task AddRatingAsync_ValidRating_ShouldAddRating()
        {
            // Arrange
            var ratingDto = new RatingDto
            {
                UserId = 1,
                ServiceProviderId = 1,
                RatingValue = 5
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync(new User { Id = 1 });
            _mockServiceProviderRepository.Setup(repo => repo.GetServiceProviderByIdAsync(1))
                .ReturnsAsync(new ServiceProvider { Id = 1 });

            // Act
            await _rateService.AddRatingAsync(ratingDto);

            // Assert
            _mockRatingRepository.Verify(repo => 
                repo.AddRatingAsync(It.Is<Rating>(r => 
                    r.UserId == ratingDto.UserId &&
                    r.ServiceProviderId == ratingDto.ServiceProviderId &&
                    r.RatingValue == ratingDto.RatingValue)), 
                Times.Once);
        }

        [Fact]
        public async Task AddRatingAsync_UserNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            var ratingDto = new RatingDto
            {
                UserId = 1,
                ServiceProviderId = 1,
                RatingValue = 5
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _rateService.AddRatingAsync(ratingDto));
            Assert.Equal("User not found.", exception.Message);
        }

        [Fact]
        public async Task AddRatingAsync_ServiceProviderNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            var ratingDto = new RatingDto
            {
                UserId = 1,
                ServiceProviderId = 1,
                RatingValue = 5
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync(new User { Id = 1 });
            _mockServiceProviderRepository.Setup(repo => repo.GetServiceProviderByIdAsync(1))
                .ReturnsAsync((ServiceProvider)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _rateService.AddRatingAsync(ratingDto));
            Assert.Equal("Service provider not found.", exception.Message);
        }

        [Fact]
        public async Task AddRatingAsync_InvalidRatingValue_ShouldThrowArgumentException()
        {
            // Arrange
            var ratingDto = new RatingDto
            {
                UserId = 1,
                ServiceProviderId = 1,
                RatingValue = 6 // Invalid rating value
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync(new User { Id = 1 });
            _mockServiceProviderRepository.Setup(repo => repo.GetServiceProviderByIdAsync(1))
                .ReturnsAsync(new ServiceProvider { Id = 1 });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _rateService.AddRatingAsync(ratingDto));
            Assert.Equal("Rating value must be between 1 and 5.", exception.Message);
        }

        [Fact]
        public async Task GetAverageRatingAsync_ValidServiceProvider_ShouldReturnAverageRating()
        {
            // Arrange
            var serviceProviderId = 1;
            var ratings = new List<Rating>
            {
                new Rating { RatingValue = 5 },
                new Rating { RatingValue = 4 },
                new Rating { RatingValue = 3 }
            };

            _mockRatingRepository.Setup(repo => repo.GetRatingsByServiceProviderIdAsync(serviceProviderId))
                .ReturnsAsync(ratings);

            // Act
            var result = await _rateService.GetAverageRatingAsync(serviceProviderId);

            // Assert
            Assert.Equal(4, result);
        }

        [Fact]
        public async Task GetAverageRatingAsync_NoRatings_ShouldReturnNull()
        {
            // Arrange
            var serviceProviderId = 1;
            _mockRatingRepository.Setup(repo => repo.GetRatingsByServiceProviderIdAsync(serviceProviderId))
                .ReturnsAsync(new List<Rating>());

            // Act
            var result = await _rateService.GetAverageRatingAsync(serviceProviderId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAverageRatingAsync_InvalidServiceProvider_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidServiceProviderId = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _rateService.GetAverageRatingAsync(invalidServiceProviderId));
            Assert.Equal("Invalid service provider ID.", exception.Message);
        }
    }
}
