using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using RatingService.Controller;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Tests.Controllers
{
    public class RatingControllerTests
    {
        private readonly Mock<IRateService> _mockRatingService;
        private readonly RatingController _controller;

        public RatingControllerTests()
        {
            _mockRatingService = new Mock<IRateService>();
            _controller = new RatingController(_mockRatingService.Object);
        }

        [Fact]
        public async Task AddRating_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Rating", "Rating is required");

            var result = await _controller.AddRating(null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task AddRating_ReturnsOk_WhenRatingIsAddedSuccessfully()
        {
            var ratingDto = new RatingDto { RatingValue = 5, ServiceProviderId = 1, UserId = 1 };
            _mockRatingService.Setup(service => service.AddRatingAsync(ratingDto)).Returns(Task.CompletedTask);

            var result = await _controller.AddRating(ratingDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task AddRating_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            var ratingDto = new RatingDto { RatingValue = 5, ServiceProviderId = 1, UserId = 1 };
            _mockRatingService.Setup(service => service.AddRatingAsync(ratingDto)).ThrowsAsync(new System.Exception("Database error"));

            var result = await _controller.AddRating(ratingDto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            var returnValue = objectResult.Value as dynamic;
            Assert.Equal("Internal server error", returnValue?.Message);
            Assert.Equal("Database error", returnValue?.Detail);
        }
    }
}
