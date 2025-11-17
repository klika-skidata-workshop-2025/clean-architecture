using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ServiceTemplate.Application.Common.Behaviors;

namespace ServiceTemplate.Application;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services (MediatR, FluentValidation, behaviors).
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR with all handlers in this assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Register pipeline behaviors (order matters!)
            // 1. Validation runs first
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

            // 2. Add other behaviors here (logging, performance monitoring, etc.)
        });

        // Register all FluentValidation validators in this assembly
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
