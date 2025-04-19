using Microsoft.AspNetCore.Mvc;
using Moq;
using RatingService.Api.Controllers;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;
using Xunit;

namespace RatingService.Tests
{
    public class ServiceProviderControllerTests
    {
        private readonly Mock<IProviderService> _mockProviderService;
        private readonly ServiceProviderController _controller;

        public ServiceProviderControllerTests()
        {
            _mockProviderService = new Mock<IProviderService>();
            _controller = new ServiceProviderController(_mockProviderService.Object);
        }

        [Fact]
        public async Task GetServiceProvider_ReturnsOkResult_WithServiceProvider()
        {
            // Arrange
            var serviceProviderId = 1;
            var serviceProvider = new ServiceProvider
            {
                Id = serviceProviderId,
                Name = "Test Provider",
                Description = "Test Description"
            };

            _mockProviderService
                .Setup(service => service.GetServiceProviderAsync(serviceProviderId))
                .ReturnsAsync(serviceProvider);

            // Act
            var result = await _controller.GetServiceProvider(serviceProviderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ServiceProviderDto>(okResult.Value);
            Assert.Equal(serviceProviderId, returnValue.Id);
            Assert.Equal(serviceProvider.Name, returnValue.Name);
            Assert.Equal(serviceProvider.Description, returnValue.Description);
        }

        [Fact]
        public async Task GetServiceProvider_ReturnsNotFound_WhenServiceProviderNotFound()
        {
            // Arrange
            var serviceProviderId = 1;
            _mockProviderService
                .Setup(service => service.GetServiceProviderAsync(serviceProviderId))
                .ReturnsAsync((ServiceProvider)null);

            // Act
            var result = await _controller.GetServiceProvider(serviceProviderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Service provider not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetServiceProvider_ReturnsBadRequest_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _controller.GetServiceProvider(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid service provider ID.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddServiceProvider_ReturnsOkResult_WhenServiceProviderIsAdded()
        {
            // Arrange
            var serviceProviderDto = new ServiceProviderDto
            {
                Name = "Test Provider",
                Description = "Test Description"
            };

            // Act
            var result = await _controller.AddServiceProvider(serviceProviderDto);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _mockProviderService.Verify(service => service.AddServiceProviderAsync(serviceProviderDto), Times.Once);
        }

        [Fact]
        public async Task AddServiceProvider_ReturnsBadRequest_WhenServiceProviderIsNull()
        {
            // Act
            var result = await _controller.AddServiceProvider(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Service provider cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddServiceProvider_ReturnsBadRequest_WhenServiceProviderIsInvalid()
        {
            // Arrange
            var invalidServiceProvider = new ServiceProviderDto
            {
                Name = "", // Invalid name
                Description = "Test Description"
            };

            // Act
            var result = await _controller.AddServiceProvider(invalidServiceProvider);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid service provider data.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAllServiceProviders_ReturnsOkResult_WithServiceProviders()
        {
            // Arrange
            var serviceProviders = new List<ServiceProvider>
            {
                new ServiceProvider { Id = 1, Name = "Provider 1", Description = "Description 1" },
                new ServiceProvider { Id = 2, Name = "Provider 2", Description = "Description 2" }
            };

            _mockProviderService
                .Setup(service => service.GetAllServiceProvidersAsync())
                .ReturnsAsync(serviceProviders);

            // Act
            var result = await _controller.GetAllServiceProviders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ServiceProviderDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetAllServiceProviders_ReturnsNotFound_WhenNoServiceProvidersFound()
        {
            // Arrange
            _mockProviderService
                .Setup(service => service.GetAllServiceProvidersAsync())
                .ReturnsAsync(new List<ServiceProvider>());

            // Act
            var result = await _controller.GetAllServiceProviders();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No service providers found.", notFoundResult.Value);
        }
    }
}
