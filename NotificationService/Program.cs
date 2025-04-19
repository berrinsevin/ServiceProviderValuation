using Serilog;
using NotificationService.Workers;
using Serilog.Sinks.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using ServiceProviderRatingNuget.DataAccess;
using NotificationService.Business.Services;
using NotificationService.Infrastructure.Messaging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext for Entity Framework Core
builder.Services.AddDbContext<ServiceProviderRatingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Serilog for logging

// This configuration initializes Serilog using settings from the appsettings.json file.
// It allows dynamic updates to logging configuration without modifying the code.
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// This configuration initializes the logging system to write logs to both the console and Elasticsearch.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ApplicationName", "NotificationService")
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

// Dependency Injection for Services
builder.Services.AddScoped<IRateNotificationService, RateNotificationService>();
builder.Services.AddHostedService<NotificationWorker>();

// RabbitMQ dependencies
builder.Services.AddSingleton(sp => 
    new RabbitMqConnectionFactory(
        hostName: builder.Configuration["RabbitMq:HostName"], 
        userName: builder.Configuration["RabbitMq:UserName"], 
        password: builder.Configuration["RabbitMq:Password"]));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors("AllowAll");

// Use Exception Handling
app.UseExceptionHandler("/error");

// Use Health Checks
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
