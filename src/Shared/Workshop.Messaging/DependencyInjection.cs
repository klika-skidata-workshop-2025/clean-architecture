using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workshop.Messaging.Abstractions;
using Workshop.Messaging.Configuration;
using Workshop.Messaging.Implementation;

namespace Workshop.Messaging;

/// <summary>
/// Extension methods for registering RabbitMQ messaging services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds RabbitMQ publisher to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration containing RabbitMQ settings</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// In Program.cs or Startup.cs:
    /// <code>
    /// builder.Services.AddRabbitMQPublisher(builder.Configuration);
    /// </code>
    ///
    /// In appsettings.json:
    /// <code>
    /// {
    ///   "RabbitMQ": {
    ///     "Host": "localhost",
    ///     "Port": 5672,
    ///     "Username": "workshop",
    ///     "Password": "workshop123"
    ///   }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register RabbitMQ configuration
        services.Configure<RabbitMQOptions>(
            configuration.GetSection(RabbitMQOptions.SectionName));

        // Register publisher as singleton (maintains persistent connection)
        services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

        return services;
    }

    /// <summary>
    /// Adds RabbitMQ publisher with custom options.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure RabbitMQ options</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddRabbitMQPublisher(options =>
    /// {
    ///     options.Host = "rabbitmq.production.com";
    ///     options.Port = 5672;
    ///     options.Username = "prod-user";
    ///     options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
    ///     options.ExchangeName = "production.events";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddRabbitMQPublisher(
        this IServiceCollection services,
        Action<RabbitMQOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

        return services;
    }
}
