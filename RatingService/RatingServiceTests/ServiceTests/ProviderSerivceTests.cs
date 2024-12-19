using Moq;
using Xunit;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;
using ServiceProviderRatingNuget.DataAccess.Repositories;

public class ProviderServiceTests
{
    private readonly Mock<IServiceProviderRepository> _mockProviderRepository;
    private readonly ProviderService _service;

    public ProviderServiceTests()
    {
        _mockProviderRepository = new Mock<IServiceProviderRepository>();
        _service = new ProviderService(_mockProviderRepository.Object);
    }

    [Fact]
    public async Task GetProvidersAsync_ReturnsProviders()
    {
        var providers = new List<Provider> { new Provider { Id = 1, Name = "Provider1" } };
        _mockProviderRepository.Setup(repo => repo.GetProvidersAsync()).ReturnsAsync(providers);

        var result = await _service.GetProvidersAsync();

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetProviderByIdAsync_ReturnsProvider_WhenProviderExists()
    {
        var provider = new Provider { Id = 1, Name = "Provider1" };
        _mockProviderRepository.Setup(repo => repo.GetProviderByIdAsync(1)).ReturnsAsync(provider);

        var result = await _service.GetProviderByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(provider.Id, result.Id);
    }

    [Fact]
    public async Task GetProviderByIdAsync_ThrowsArgumentException_WhenIdIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetProviderByIdAsync(0));
    }

    [Fact]
    public async Task AddProviderAsync_AddsProvider_WhenProviderIsValid()
    {
        var providerDto = new ProviderDto { Name = "NewProvider" };
        _mockProviderRepository.Setup(repo => repo.IsProviderWithSameNameExistsAsync(providerDto.Name)).ReturnsAsync(false);

        await _service.AddProviderAsync(providerDto);

        _mockProviderRepository.Verify(repo => repo.AddProviderAsync(It.IsAny<Provider>()), Times.Once);
    }

    [Fact]
    public async Task AddProviderAsync_ThrowsInvalidOperationException_WhenProviderExists()
    {
        var providerDto = new ProviderDto { Name = "ExistingProvider" };
        _mockProviderRepository.Setup(repo => repo.IsProviderWithSameNameExistsAsync(providerDto.Name)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddProviderAsync(providerDto));
    }

    [Fact]
    public async Task UpdateProviderAsync_UpdatesProvider_WhenProviderIsValid()
    {
        var provider = new Provider { Id = 1, Name = "UpdatedProvider" };
        _mockProviderRepository.Setup(repo => repo.UpdateProviderAsync(It.IsAny<Provider>())).Returns(Task.CompletedTask);

        await _service.UpdateProviderAsync(provider);

        _mockProviderRepository.Verify(repo => repo.UpdateProviderAsync(It.IsAny<Provider>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProviderAsync_ThrowsArgumentNullException_WhenProviderIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateProviderAsync(null));
    }

    [Fact]
    public async Task DeleteProviderAsync_DeletesProvider_WhenProviderExists()
    {
        var providerId = 1;
        _mockProviderRepository.Setup(repo => repo.DeleteProviderAsync(providerId)).Returns(Task.CompletedTask);

        await _service.DeleteProviderAsync(providerId);

        _mockProviderRepository.Verify(repo => repo.DeleteProviderAsync(providerId), Times.Once);
    }

    [Fact]
    public async Task DeleteProviderAsync_ThrowsArgumentException_WhenIdIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteProviderAsync(0));
    }
}
