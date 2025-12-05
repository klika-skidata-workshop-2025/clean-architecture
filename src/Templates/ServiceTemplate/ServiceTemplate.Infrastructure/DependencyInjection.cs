using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceTemplate.Application.Common.Interfaces;
using ServiceTemplate.Infrastructure.Persistence;
using Workshop.Messaging;

namespace ServiceTemplate.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });

            // Enable detailed errors in development
            if (configuration.GetValue<bool>("DetailedErrors"))
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        // Register DbContext as IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // RabbitMQ Publisher
        services.AddRabbitMQPublisher(configuration);

        // TODO: Add RabbitMQ consumers here
        // Example:
        // services.AddHostedService<YourEventConsumer>();

        return services;
    }

    /// <summary>
    /// Ensures the database is created and migrations are applied.
    /// Call this during application startup.
    /// </summary>
    /// <param name="services">Service provider</param>
    /// <returns>Task</returns>
    public static async Task EnsureDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Apply pending migrations
        await context.Database.MigrateAsync();
    }
}
