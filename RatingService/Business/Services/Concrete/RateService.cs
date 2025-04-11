using Serilog;
using System.Text.Json;
using StackExchange.Redis;
using RatingService.Domain.Entities.Enums;
using RatingService.Infrastructure.Messaging;
using Microsoft.Extensions.Caching.Distributed;
using ServiceProviderRatingNuget.Domain.Entities;
using ServiceProviderRatingNuget.DataAccess.Repositories;

namespace RatingService.Business.Services
{
    public class RateService : IRateService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IDistributedCache _cache;
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly RabbitMqClient _notificationRabbitMqClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateService"/> class
        /// </summary>
        /// <param name="ratingRepository">The rating repository</param>
        /// <param name="cache">The distributed cache</param>
        /// <param name="notificationRabbitMqClient">The RabbitMQ client for notifications</param>
        public RateService(IRatingRepository ratingRepository, IDistributedCache cache, ConnectionMultiplexer redisConnection, RabbitMqClient notificationRabbitMqClient)
        {
            _ratingRepository = ratingRepository;
            _cache = cache;
            _redisConnection = redisConnection;
            _notificationRabbitMqClient = notificationRabbitMqClient;
        }

        /// <summary>
        /// Adds a new rating asynchronously
        /// </summary>
        /// <param name="rating">The rating to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the rating is null</exception>
        /// <exception cref="ArgumentException">Thrown when the rating value is not between 1 and 5</exception>
        /// <exception cref="Exception">Thrown when an error occurs while adding the rating</exception>
        public async Task AddRatingAsync(RatingDto rating)
        {
            if (rating == null)
            {
                throw new ArgumentNullException(nameof(rating), "Rating cannot be null.");
            }

            if (rating.ServiceProviderId <= 0)
            {
                throw new ArgumentException("ServiceProviderId must be a valid value.", nameof(rating.ServiceProviderId));
            }

            if (!Enum.IsDefined(typeof(RatingValue), rating.RatingValue))
            {
                throw new ArgumentException("Rating value must be between 1 and 5.", nameof(rating.RatingValue));
            }

            try
            {
                await _ratingRepository.AddRatingAsync(new()
                {
                    UserId = rating.UserId,
                    RatingValue = rating.RatingValue,
                    ServiceProviderId = rating.ServiceProviderId,
                    CreatedDate = DateTime.Now
                });

                await _cache.RemoveAsync($"AverageRating_{rating.ServiceProviderId}");

                var message = new
                {
                    rating.ServiceProviderId,
                    rating.RatingValue,
                    rating.UserId
                };

                var messageJson = JsonSerializer.Serialize(message);
                var key = $"rating_message_{Guid.NewGuid()}";
                await _cache.SetStringAsync(key, messageJson);

                var db = _redisConnection.GetDatabase();
                await db.SetAddAsync("rating_keys", key);

                // RabbitMQ Event Publish
                var rateEvent = new RateCreatedEvent
                {
                    ProviderId = rating.ServiceProviderId,
                    RatingValue = rating.RatingValue,
                    UserId = rating.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                _notificationRabbitMqClient.PublishRateCreatedEvent(rateEvent);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding rating for provider {ServiceProviderId}", rating.ServiceProviderId);
                throw;
            }
        }
    }
}
