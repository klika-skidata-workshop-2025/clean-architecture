using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceTemplate.Client;

/// <summary>
/// Dependency injection extensions for ServiceTemplate client.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds ServiceTemplate gRPC client to the dependency injection container.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceUrl">ServiceTemplate service URL (e.g., "http://localhost:5001")</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddServiceTemplateClient(
        this IServiceCollection services,
        string serviceUrl)
    {
        // TODO: Add gRPC client registration here
        // Example:
        // services.AddGrpcClient<YourService.YourServiceClient>(options =>
        // {
        //     options.Address = new Uri(serviceUrl);
        // })
        // .ConfigureChannel(options =>
        // {
        //     // Configure retry policy
        //     options.MaxRetryAttempts = 3;
        //     options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16 MB
        //     options.MaxSendMessageSize = 16 * 1024 * 1024; // 16 MB
        // });

        // Register the client wrapper
        services.AddScoped<ServiceTemplateClient>();

        return services;
    }

    /// <summary>
    /// Adds ServiceTemplate gRPC client with advanced configuration.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceUrl">ServiceTemplate service URL</param>
    /// <param name="configureClient">Client configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddServiceTemplateClient(
        this IServiceCollection services,
        string serviceUrl,
        Action<GrpcClientFactoryOptions> configureClient)
    {
        // TODO: Add gRPC client registration with configuration here
        // Example:
        // services.AddGrpcClient<YourService.YourServiceClient>(options =>
        // {
        //     options.Address = new Uri(serviceUrl);
        //     configureClient(options);
        // });

        // Register the client wrapper
        services.AddScoped<ServiceTemplateClient>();

        return services;
    }
}
