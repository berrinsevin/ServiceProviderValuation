using Serilog;
using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Hosting;
using RatingService.Business.Services;
using Microsoft.Extensions.Caching.Distributed;
using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Business.Workers
{
    /// <summary>
    /// Worker service to handle rating calculation messages from RabbitMQ.
    /// </summary>
    public class RatingWorker : BackgroundService
    {
        private readonly IProviderService _providerService;
        private readonly IDistributedCache _cache;
        private readonly ConnectionMultiplexer _redisConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingCalculationWorker"/> class
        /// </summary>
        /// <param name="providerService">The provider service.</param>
        /// <param name="_cache">The distributed cache</param>
        public RatingWorker(IProviderService providerService, IDistributedCache cache, ConnectionMultiplexer redisConnection)
        {
            _providerService = providerService;
            _cache = cache;
            _redisConnection = redisConnection;
        }

        /// <summary>
        /// Executes the worker service.
        /// </summary>
        /// <param name="stoppingToken">The token used to stop the service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redisConnection.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                var keys = await db.SetMembersAsync("rating_keys");

                foreach (var key in keys)
                {
                    var messageJson = await _cache.GetStringAsync(key);
                    if (!string.IsNullOrEmpty(messageJson))
                    {
                        var message = JsonSerializer.Deserialize<RatingMessage>(messageJson);

                        // Limits daily rating 
                        // This ensures that users cannot submit more than a specified number of ratings within a day and protecting the system from being overloaded with requests
                        if (await IsRatingLimitExceededAsync(message.ServiceProviderId, db))
                        {
                            Log.Warning("Daily rating limit exceeded for provider ID {ServiceProviderId}", message.ServiceProviderId);
                        }
                        else
                        {
                            await CalculateAndUpdateAverageRating(message.ServiceProviderId, message.RatingValue);
                            await IncrementDailyRatingCountAsync(message.ServiceProviderId, db);
                        }

                        await db.SetRemoveAsync("rating_keys", key);
                        await _cache.RemoveAsync(key);
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        /// <summary>
        /// Calculates and updates the average rating for a service provider.
        /// </summary>
        /// <param name="serviceProviderId">The ID of the service provider.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while calculating or updating the average rating.</exception>
        private async Task CalculateAndUpdateAverageRating(int serviceProviderId, int newRatingValue)
        {
            try
            {
                var provider = await _providerService.GetProviderByIdAsync(serviceProviderId);
                if (provider == null)
                {
                    Log.Warning("Provider with ID {ServiceProviderId} not found.", serviceProviderId);
                    return;
                }

                provider.AverageRating = (provider.AverageRating * provider.RatingCount + newRatingValue) / (provider.RatingCount + 1);
                provider.RatingCount++;
                provider.LastUpdatedDate = DateTime.Now;

                await _providerService.UpdateProviderAsync(provider);
                Log.Information("Updated average rating for provider with ID {ServiceProviderId}", serviceProviderId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the average rating for provider with ID {ServiceProviderId}", serviceProviderId);
                throw;
            }
        }

        /// <summary>
        /// Checks if the daily rating limit has been exceeded for a provider.
        /// </summary>
        /// <param name="serviceProviderId">The ID of the service provider.</param>
        /// <param name="db">The Redis database.</param>
        /// <returns>True if the daily rating limit has been exceeded, otherwise false.</returns>
        private async Task<bool> IsRatingLimitExceededAsync(int serviceProviderId, IDatabase db)
        {
            var key = GetDailyRatingKey(serviceProviderId);
            var count = await db.StringGetAsync(key);
            if (!count.IsNull)
            {
                if (int.TryParse(count, out int currentCount))
                {
                    return currentCount >= RatingService.Domain.Entities.Constants.DailyRatingLimit;
                }
            }
            return false;
        }

        /// <summary>
        /// Increments the daily rating count for a provider.
        /// </summary>
        /// <param name="serviceProviderId">The ID of the service provider.</param>
        /// <param name="db">The Redis database.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task IncrementDailyRatingCountAsync(int serviceProviderId, IDatabase db)
        {
            var key = GetDailyRatingKey(serviceProviderId);
            await db.StringIncrementAsync(key);

            var expiry = DateTime.UtcNow.Date.AddDays(1) - DateTime.Now;
            await db.KeyExpireAsync(key, expiry);
        }

        /// <summary>
        /// Gets the Redis key for daily rating count of a provider.
        /// </summary>
        /// <param name="serviceProviderId">The ID of the service provider.</param>
        /// <returns>The Redis key as a string.</returns>
        private string GetDailyRatingKey(int serviceProviderId)
        {
            return $"daily_rating_count:{serviceProviderId}:{DateTime.UtcNow:yyyyMMdd}";
        }
    }
}
