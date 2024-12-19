using Microsoft.AspNetCore.Mvc;
using Moq;
using RatingService.Api.Controllers;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;
using Xunit;

public class ProviderControllerTests
{
    private readonly Mock<IProviderService> _mockProviderService;
    private readonly ProviderController _controller;

    public ProviderControllerTests()
    {
        _mockProviderService = new Mock<IProviderService>();
        _controller = new ProviderController(_mockProviderService.Object);
    }

    [Fact]
    public async Task GetProviders_ReturnsOkResult_WithListOfProviders()
    {
        var providers = new List<ProviderDto>
        {
            new ProviderDto { Id = 1, Name = "Provider 1" },
            new ProviderDto { Id = 2, Name = "Provider 2" }
        };

        _mockProviderService.Setup(service => service.GetProvidersAsync()).Returns(Task.FromResult((IEnumerable<Provider>)providers));

        var result = await _controller.GetProviders();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<ProviderDto>>(okResult.Value);
        Assert.Equal(providers.Count, returnValue.Count);
    }

    [Fact]
    public async Task GetProviderById_ReturnsOkResult_WithProvider()
    {
        var provider = new Provider { Id = 1, Name = "Provider 1" };

        _mockProviderService.Setup(service => service.GetProviderByIdAsync(1)).Returns(Task.FromResult(provider));

        var result = await _controller.GetProviderById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ProviderDto>(okResult.Value);
        Assert.Equal(provider.Id, returnValue.Id);
    }

    [Fact]
    public async Task GetProviderById_ReturnsNotFound_WhenProviderDoesNotExist()
    {
        _mockProviderService.Setup(service => service.GetProviderByIdAsync(1)).ReturnsAsync((Provider)null);

        var result = await _controller.GetProviderById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddProvider_ReturnsCreatedAtAction_WhenProviderIsAdded()
    {
        var provider = new Provider { Id = 1, Name = "Provider 1" };
        var providerDto = new ProviderDto { Id = 1, Name = "Provider 1" };

        _mockProviderService.Setup(service => service.AddProviderAsync(It.IsAny<ProviderDto>())).Returns(Task.FromResult(provider));

        var result = await _controller.AddProvider(providerDto);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnValue = Assert.IsType<ProviderDto>(createdAtActionResult.Value);
        Assert.Equal(provider.Id, returnValue.Id);
    }

    [Fact]
    public async Task AddProvider_ReturnsBadRequest_WhenInvalidOperationExceptionOccurs()
    {
        var provider = new ProviderDto { Id = 1, Name = "Provider 1" };

        _mockProviderService.Setup(service => service.AddProviderAsync(provider)).ThrowsAsync(new InvalidOperationException("Invalid operation"));

        var result = await _controller.AddProvider(provider);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<string>(badRequestResult.Value);
        Assert.Equal("Invalid operation", returnValue);
    }

    [Fact]
    public async Task DeleteProvider_ReturnsNoContent_WhenProviderIsDeleted()
    {
        var providerId = 1;

        _mockProviderService.Setup(service => service.DeleteProviderAsync(providerId)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteProvider(providerId);

        Assert.IsType<NoContentResult>(result);
    }
}
