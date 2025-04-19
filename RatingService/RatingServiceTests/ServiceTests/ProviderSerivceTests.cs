using Moq;
using Xunit;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;
using ServiceProviderRatingNuget.DataAccess.Repositories;

namespace RatingService.Tests
{
    public class ProviderServiceTests
    {
        private readonly Mock<IServiceProviderRepository> _mockServiceProviderRepository;
        private readonly ProviderService _providerService;

        public ProviderServiceTests()
        {
            _mockServiceProviderRepository = new Mock<IServiceProviderRepository>();
            _providerService = new ProviderService(_mockServiceProviderRepository.Object);
        }

        [Fact]
        public async Task GetServiceProviderAsync_ValidId_ShouldReturnServiceProvider()
        {
            // Arrange
            var serviceProviderId = 1;
            var serviceProvider = new ServiceProvider
            {
                Id = serviceProviderId,
                Name = "Test Provider",
                Description = "Test Description"
            };

            _mockServiceProviderRepository
                .Setup(repo => repo.GetServiceProviderByIdAsync(serviceProviderId))
                .ReturnsAsync(serviceProvider);

            // Act
            var result = await _providerService.GetServiceProviderAsync(serviceProviderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(serviceProviderId, result.Id);
            Assert.Equal(serviceProvider.Name, result.Name);
            Assert.Equal(serviceProvider.Description, result.Description);
        }

        [Fact]
        public async Task GetServiceProviderAsync_InvalidId_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidId = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _providerService.GetServiceProviderAsync(invalidId));
            Assert.Equal("Invalid service provider ID.", exception.Message);
        }

        [Fact]
        public async Task GetServiceProviderAsync_NotFound_ShouldReturnNull()
        {
            // Arrange
            var serviceProviderId = 1;
            _mockServiceProviderRepository
                .Setup(repo => repo.GetServiceProviderByIdAsync(serviceProviderId))
                .ReturnsAsync((ServiceProvider)null);

            // Act
            var result = await _providerService.GetServiceProviderAsync(serviceProviderId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddServiceProviderAsync_ValidProvider_ShouldAddProvider()
        {
            // Arrange
            var serviceProviderDto = new ServiceProviderDto
            {
                Name = "Test Provider",
                Description = "Test Description"
            };

            // Act
            await _providerService.AddServiceProviderAsync(serviceProviderDto);

            // Assert
            _mockServiceProviderRepository.Verify(repo => 
                repo.AddServiceProviderAsync(It.Is<ServiceProvider>(p => 
                    p.Name == serviceProviderDto.Name &&
                    p.Description == serviceProviderDto.Description)), 
                Times.Once);
        }

        [Fact]
        public async Task AddServiceProviderAsync_NullProvider_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _providerService.AddServiceProviderAsync(null));
            Assert.Equal("Service provider cannot be null. (Parameter 'serviceProvider')", exception.Message);
        }

        [Fact]
        public async Task AddServiceProviderAsync_InvalidProvider_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidProvider = new ServiceProviderDto
            {
                Name = "", // Invalid name
                Description = "Test Description"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _providerService.AddServiceProviderAsync(invalidProvider));
            Assert.Equal("Service provider name cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task GetAllServiceProvidersAsync_ShouldReturnAllProviders()
        {
            // Arrange
            var serviceProviders = new List<ServiceProvider>
            {
                new ServiceProvider { Id = 1, Name = "Provider 1", Description = "Description 1" },
                new ServiceProvider { Id = 2, Name = "Provider 2", Description = "Description 2" }
            };

            _mockServiceProviderRepository
                .Setup(repo => repo.GetAllServiceProvidersAsync())
                .ReturnsAsync(serviceProviders);

            // Act
            var result = await _providerService.GetAllServiceProvidersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(serviceProviders[0].Name, result.First().Name);
            Assert.Equal(serviceProviders[1].Name, result.Last().Name);
        }

        [Fact]
        public async Task GetAllServiceProvidersAsync_NoProviders_ShouldReturnEmptyList()
        {
            // Arrange
            _mockServiceProviderRepository
                .Setup(repo => repo.GetAllServiceProvidersAsync())
                .ReturnsAsync(new List<ServiceProvider>());

            // Act
            var result = await _providerService.GetAllServiceProvidersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
