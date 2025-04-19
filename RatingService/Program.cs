using Serilog;
using Serilog.Sinks.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using RatingService.Business.Workers;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.DataAccess;
using ServiceProviderRatingNuget.Extensions;
using RatingService.Infrastructure.Messaging;
using ServiceProviderRatingNuget.DataAccess.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using AspNetCoreRateLimit;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure DbContext for Entity Framework Core
        builder.Services.AddDbContext<ServiceProviderRatingDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Configure Redis Cache
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
            options.InstanceName = "RatingService_";
        });

        // Configure Serilog for logging

        // This configuration initializes Serilog using settings from the appsettings.json file.
        // It allows dynamic updates to logging configuration without modifying the code.
        builder.Host.UseSerilog((context, config) =>
            config.ReadFrom.Configuration(context.Configuration));

        // This configuration initializes the logging system to write logs to both the console and Elasticsearch.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("ApplicationName", "RatingService")
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["ElasticConfiguration:Uri"]))
            {
                AutoRegisterTemplate = true,
                FailureCallback = (logEvent, exception) => Console.WriteLine("Elasticsearch connection error: " + exception.Message) // Handle connection errors
            })
            .CreateLogger();

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add Health Checks
        builder.Services.AddHealthChecks()
            .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            .AddRedis(builder.Configuration.GetConnectionString("RedisConnection"))
            .AddRabbitMQ(
                rabbitConnectionString: $"amqp://{builder.Configuration["RabbitMq:UserName"]}:{builder.Configuration["RabbitMq:Password"]}@{builder.Configuration["RabbitMq:HostName"]}");

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        // Add Rate Limiting
        builder.Services.AddMemoryCache();
        builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
        builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        builder.Services.AddInMemoryRateLimiting();

        // Dependency Injection for Repositories
        builder.Services.AddScoped<IRatingRepository, RatingRepository>();
        builder.Services.AddScoped<IServiceProviderRepository, ServiceProviderRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Dependency Injection for Services
        builder.Services.AddScoped<IRateService, RateService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IProviderService, ProviderService>();
        builder.Services.AddHostedService<RatingWorker>();

        // RabbitMQ dependencies
        builder.Services.AddSingleton<RabbitMqClient>(sp =>
        {
            var settings = builder.Configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>();
            return new RabbitMqClient(settings);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.(Open API)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Use CORS
        app.UseCors("AllowAll");

        // Use Rate Limiting
        app.UseIpRateLimiting();

        app.UseExceptionHandling();

        // Use Health Checks
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}