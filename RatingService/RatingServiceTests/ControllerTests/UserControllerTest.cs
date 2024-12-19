using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using RatingService.Controller;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;

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
    public async Task GetUserById_ReturnsOkResult_WithUser()
    {
        var user = new User { Id = 1, Name = "John Doe" };

        _mockUserService.Setup(service => service.GetUserByIdAsync(1)).Returns(Task.FromResult(user));

        var result = await _controller.GetUserById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(user.Id, returnValue.Id);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _mockUserService.Setup(service => service.GetUserByIdAsync(1)).Returns(Task.FromResult((User)null));

        var result = await _controller.GetUserById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddUser_ReturnsCreatedAtAction_WhenUserIsAdded()
    {
        var user = new User { Id = 1, Name = "John Doe" };
        var userDto = new UserDto { Id = 1, Name = "John Doe" };

        _mockUserService.Setup(service => service.AddUserAsync(It.IsAny<UserDto>())).Returns(Task.FromResult((User)user));

        var result = await _controller.AddUser(userDto);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnValue = Assert.IsType<UserDto>(createdAtActionResult.Value);
        Assert.Equal(user.Id, returnValue.Id);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNoContent_WhenUserIsDeleted()
    {
        var userId = 1;

        _mockUserService.Setup(service => service.DeleteUserAsync(userId)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteUser(userId);

        Assert.IsType<NoContentResult>(result);
    }
}
