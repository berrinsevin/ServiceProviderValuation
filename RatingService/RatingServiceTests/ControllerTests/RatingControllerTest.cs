using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using RatingService.Controller;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Tests
{
    public class RatingControllerTests
    {
        private readonly Mock<IRateService> _mockRateService;
        private readonly RatingController _controller;

        public RatingControllerTests()
        {
            _mockRateService = new Mock<IRateService>();
            _controller = new RatingController(_mockRateService.Object);
        }

        [Fact]
        public async Task AddRating_ReturnsOkResult_WhenRatingIsAdded()
        {
            // Arrange
            var ratingDto = new RatingDto
            {
                RatingValue = 5,
                ServiceProviderId = 1,
                UserId = 1
            };

            // Act
            var result = await _controller.AddRating(ratingDto);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _mockRateService.Verify(service => service.AddRatingAsync(ratingDto), Times.Once);
        }

        [Fact]
        public async Task AddRating_ReturnsBadRequest_WhenRatingIsNull()
        {
            // Act
            var result = await _controller.AddRating(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Rating cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddRating_ReturnsBadRequest_WhenRatingIsInvalid()
        {
            // Arrange
            var invalidRating = new RatingDto
            {
                RatingValue = 6, // Invalid rating value
                ServiceProviderId = 0, // Invalid service provider ID
                UserId = 0 // Invalid user ID
            };

            // Act
            var result = await _controller.AddRating(invalidRating);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid rating data.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAverageRating_ReturnsOkResult_WithAverageRating()
        {
            // Arrange
            var serviceProviderId = 1;
            var averageRating = 4.5;

            _mockRateService
                .Setup(service => service.GetAverageRatingAsync(serviceProviderId))
                .ReturnsAsync(averageRating);

            // Act
            var result = await _controller.GetAverageRating(serviceProviderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(averageRating, okResult.Value);
        }

        [Fact]
        public async Task GetAverageRating_ReturnsNotFound_WhenNoRatingsFound()
        {
            // Arrange
            var serviceProviderId = 1;
            _mockRateService
                .Setup(service => service.GetAverageRatingAsync(serviceProviderId))
                .ReturnsAsync((double?)null);

            // Act
            var result = await _controller.GetAverageRating(serviceProviderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No ratings found for this service provider.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAverageRating_ReturnsBadRequest_WhenServiceProviderIdIsInvalid()
        {
            // Arrange
            var invalidServiceProviderId = 0;

            // Act
            var result = await _controller.GetAverageRating(invalidServiceProviderId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid service provider ID.", badRequestResult.Value);
        }
    }
}
