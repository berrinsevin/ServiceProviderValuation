using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using RatingService.Controller;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        [Fact]
        public async Task GetUser_ReturnsOkResult_WithUser()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@example.com"
            };

            _mockUserService
                .Setup(service => service.GetUserAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userId, returnValue.Id);
            Assert.Equal(user.Name, returnValue.Name);
            Assert.Equal(user.Email, returnValue.Email);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            var userId = 1;
            _mockUserService
                .Setup(service => service.GetUserAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetUser_ReturnsBadRequest_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _controller.GetUser(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid user ID.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddUser_ReturnsOkResult_WhenUserIsAdded()
        {
            // Arrange
            var userDto = new UserDto
            {
                Name = "Test User",
                Email = "test@example.com"
            };

            // Act
            var result = await _controller.AddUser(userDto);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _mockUserService.Verify(service => service.AddUserAsync(userDto), Times.Once);
        }

        [Fact]
        public async Task AddUser_ReturnsBadRequest_WhenUserIsNull()
        {
            // Act
            var result = await _controller.AddUser(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddUser_ReturnsBadRequest_WhenUserIsInvalid()
        {
            // Arrange
            var invalidUser = new UserDto
            {
                Name = "", // Invalid name
                Email = "invalid-email" // Invalid email
            };

            // Act
            var result = await _controller.AddUser(invalidUser);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid user data.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUserLastFetchTime_ReturnsOkResult_WhenUpdated()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _controller.UpdateUserLastFetchTime(userId);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _mockUserService.Verify(service => service.UpdateUserLastFetchTimeAsync(userId), Times.Once);
        }

        [Fact]
        public async Task UpdateUserLastFetchTime_ReturnsBadRequest_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _controller.UpdateUserLastFetchTime(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid user ID.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUserLastFetchTime_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            var userId = 1;
            _mockUserService
                .Setup(service => service.UpdateUserLastFetchTimeAsync(userId))
                .ThrowsAsync(new ArgumentException("User not found."));

            // Act
            var result = await _controller.UpdateUserLastFetchTime(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }
    }
}
