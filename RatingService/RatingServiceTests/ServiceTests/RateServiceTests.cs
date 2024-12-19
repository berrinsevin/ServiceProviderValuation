using Moq;
using RatingService.Business.Services;
using RatingService.Infrastructure.Messaging;
using ServiceProviderRatingNuget.DataAccess.Repositories;
using ServiceProviderRatingNuget.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using Xunit;
using System.Threading.Tasks;

public class RateServiceTests
{
    private readonly Mock<IRatingRepository> _mockRatingRepository;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ConnectionMultiplexer> _mockRedisConnection;
    private readonly Mock<RabbitMqClient> _mockNotificationRabbitMqClient;
    private readonly RateService _service;

    public RateServiceTests()
    {
        _mockRatingRepository = new Mock<IRatingRepository>();
        _mockCache = new Mock<IDistributedCache>();
        _mockRedisConnection = new Mock<ConnectionMultiplexer>();
        _mockNotificationRabbitMqClient = new Mock<RabbitMqClient>();
        _service = new RateService(_mockRatingRepository.Object, _mockCache.Object, _mockRedisConnection.Object, _mockNotificationRabbitMqClient.Object);
    }

    [Fact]
    public async Task AddRatingAsync_AddsRating_WhenRatingIsValid()
    {
        var ratingDto = new RatingDto
        {
            UserId = 1,
            ServiceProviderId = 1,
            RatingValue = 4
        };

        _mockRatingRepository.Setup(repo => repo.AddRatingAsync(It.IsAny<Rating>())).Returns(Task.CompletedTask);
        _mockCache.Setup(cache => cache.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockCache.Setup(cache => cache.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mockDb = new Mock<IDatabase>();
        _mockRedisConnection.Setup(conn => conn.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);
        mockDb.Setup(db => db.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), CommandFlags.None)).Returns(Task.FromResult(true));

        await _service.AddRatingAsync(ratingDto);

        _mockRatingRepository.Verify(repo => repo.AddRatingAsync(It.IsAny<Rating>()), Times.Once);
        _mockCache.Verify(cache => cache.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(cache => cache.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        mockDb.Verify(db => db.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), CommandFlags.None), Times.Once);
        _mockNotificationRabbitMqClient.Verify(client => client.SendMessage(It.IsAny<string>()), Times.Once);
    }


    [Fact]
    public async Task AddRatingAsync_ThrowsArgumentNullException_WhenRatingIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddRatingAsync(null));
    }

    [Fact]
    public async Task AddRatingAsync_ThrowsArgumentException_WhenServiceProviderIdIsInvalid()
    {
        var ratingDto = new RatingDto
        {
            UserId = 1,
            ServiceProviderId = 0,
            RatingValue = 4
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRatingAsync(ratingDto));
    }

    [Fact]
    public async Task AddRatingAsync_ThrowsArgumentException_WhenUserIdIsInvalid()
    {
        var ratingDto = new RatingDto
        {
            UserId = 0,
            ServiceProviderId = 1,
            RatingValue = 4
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRatingAsync(ratingDto));
    }

    [Fact]
    public async Task AddRatingAsync_ThrowsArgumentException_WhenRatingValueIsInvalid()
    {
        var ratingDto = new RatingDto
        {
            UserId = 1,
            ServiceProviderId = 1,
            RatingValue = 6
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRatingAsync(ratingDto));
    }
}
