# Service Marketplace - Rating & Notification Service

This project is a service marketplace that allows users to rate service providers and notifies the providers when a new rating is added. The solution includes two core services:

- **Rating Service**: Allows users to submit ratings for service providers and fetch the average rating for a specific provider. It also notifies the Notification Service whenever a new rating is submitted.
  
- **Notification Service**: Allows service providers to fetch a list of new notifications that have been submitted since the last time the endpoint was called. It handles notifications in a fault-tolerant manner, meaning data may be lost if the service crashes or restarts.

## Features:
- REST APIs for submitting ratings and fetching average ratings.
- Real-time notifications using RabbitMQ to notify providers of new ratings.
- Docker and Docker Compose setup to easily build and run the services.
- Redis is used in the Rating Service for caching purposes.
- RabbitMQ is used for communication between the Rating and Notification services.
- Structured logging with Serilog for easier debugging and monitoring.
- Includes unit tests to ensure system reliability.

## Technologies:
- .NET 6.0 for both services
- RabbitMQ for message brokering between services
- Redis for caching
- Serilog for structured logging
- Docker for containerization and orchestration with Docker Compose

## Setup Instructions:
1. Clone the repository.
2. Build the solution in Visual Studio.
3. Configure `appsettings.json` with your RabbitMQ and Redis settings.
4. Use Docker Compose to build and run the services with `docker-compose up`.

## Future Improvements:
- Integration tests using Testcontainers.
- Continuous Integration (CI) pipeline setup for automated deployments.

## Decisions about Maintainability, Scalability and Reliability

### Separation of Concerns
Each component is designed with a single responsibility:
- **Controllers** handle HTTP requests and responses.
- **Business** encapsulate business logic.
- **Infrastructure** manage data access operations.
- **Domain** includes entities and helpers.

This modular architecture enhances the maintainability by making the codebase easier to navigate and update.

### Dependency Injection
Using ASP.NET Core's built-in DI container enables loose coupling of components, making it easy to replace or update implementations for testing or maintenance purposes.

### Shared Library
The shared nuget library ensures common logic, entities, and data access patterns are centralized, promoting code reuse and consistency across services.

### Structured Logging with Serilog
Structured logging is implemented with Serilog, with logs being sent to both the console and Elasticsearch. This provides comprehensive insights into application behavior, aiding in troubleshooting and monitoring.

### Exception Handling
Global exception handling ensures all unhandled exceptions are logged and appropriate responses are returned to clients. This prevents crashes and provides meaningful error messages.

### Redis for Caching
To minimize database load and improve response times, frequently accessed data such as user last fetch times are cached in Redis. This provides a fallback mechanism in case of cache invalidation.

### Microservices Architecture
The microservices architecture allows services to be independently deployed and scaled, addressing their specific performance needs and ensuring high availability.

### Asynchronous Processing with RabbitMQ
RabbitMQ is used for decoupling services and enabling asynchronous message processing. This ensures the system can handle high loads efficiently by offloading tasks to background workers.

### Rate Limiting
To prevent abuse and overload, rate-limiting mechanisms can be implemented, controlling the frequency of rating submissions and API requests.

