using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceService.Client;

/// <summary>
/// Dependency injection extensions for DeviceService client.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds DeviceService gRPC client to the dependency injection container.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceUrl">DeviceService URL (e.g., "http://localhost:5001")</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDeviceServiceClient(
        this IServiceCollection services,
        string serviceUrl)
    {
        // Register gRPC client
        services.AddGrpcClient<Workshop.Proto.Device.DeviceService.DeviceServiceClient>(options =>
        {
            options.Address = new Uri(serviceUrl);
        })
        .ConfigureChannel(options =>
        {
            // Configure retry policy
            options.MaxRetryAttempts = 3;
            options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16 MB
            options.MaxSendMessageSize = 16 * 1024 * 1024; // 16 MB
        });

        // Register the client wrapper
        services.AddScoped<DeviceServiceClient>();

        return services;
    }

    /// <summary>
    /// Adds DeviceService gRPC client with advanced configuration.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceUrl">DeviceService URL</param>
    /// <param name="configureClient">Client configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDeviceServiceClient(
        this IServiceCollection services,
        string serviceUrl,
        Action<GrpcClientFactoryOptions> configureClient)
    {
        // Register gRPC client with custom configuration
        var clientBuilder = services.AddGrpcClient<Workshop.Proto.Device.DeviceService.DeviceServiceClient>(options =>
        {
            options.Address = new Uri(serviceUrl);
            configureClient(options);
        });

        // Register the client wrapper
        services.AddScoped<DeviceServiceClient>();

        return services;
    }
}
