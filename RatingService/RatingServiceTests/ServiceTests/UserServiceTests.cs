using Moq;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.DataAccess.Repositories;
using ServiceProviderRatingNuget.Domain.Entities;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RatingService.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenUserIdIsValid()
        {
            var userId = 1;
            var expectedUser = new User { Id = userId, Name = "Test User" };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                               .ReturnsAsync(expectedUser);

            var result = await _userService.GetUserByIdAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("Test User", result.Name);
        }

        [Fact]
        public async Task GetUserByIdAsync_ThrowsArgumentException_WhenUserIdIsInvalid()
        {
            var userId = 0;

            await Assert.ThrowsAsync<ArgumentException>(() => _userService.GetUserByIdAsync(userId));
        }

        [Fact]
        public async Task AddUserAsync_AddsUser_WhenUserIsValid()
        {
            var userDto = new UserDto { Name = "New User" };

            _mockUserRepository.Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
                               .Returns(Task.CompletedTask);

            await _userService.AddUserAsync(userDto);

            _mockUserRepository.Verify(repo => repo.AddUserAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task AddUserAsync_ThrowsArgumentException_WhenUserNameIsEmpty()
        {
            var userDto = new UserDto { Name = "" };

            await Assert.ThrowsAsync<ArgumentException>(() => _userService.AddUserAsync(userDto));
        }

        [Fact]
        public async Task DeleteUserAsync_DeletesUser_WhenUserIdIsValid()
        {
            var userId = 1;

            _mockUserRepository.Setup(repo => repo.DeleteUserAsync(userId))
                               .Returns(Task.CompletedTask);

            await _userService.DeleteUserAsync(userId);

            _mockUserRepository.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ThrowsArgumentException_WhenUserIdIsInvalid()
        {
            var userId = 0;

            await Assert.ThrowsAsync<ArgumentException>(() => _userService.DeleteUserAsync(userId));
        }
    }
}
