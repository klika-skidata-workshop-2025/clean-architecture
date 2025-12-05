using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;

namespace MonitoringService.Client;

/// <summary>
/// Dependency injection extensions for MonitoringService client.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddMonitoringServiceClient(
        this IServiceCollection services,
        string serviceUrl)
    {
        services.AddGrpcClient<Workshop.Contracts.Monitoring.MonitoringService.MonitoringServiceClient>(options =>
        {
            options.Address = new Uri(serviceUrl);
        })
        .ConfigureChannel(options =>
        {
            options.MaxRetryAttempts = 3;
            options.MaxReceiveMessageSize = 16 * 1024 * 1024;
            options.MaxSendMessageSize = 16 * 1024 * 1024;
        });

        services.AddScoped<MonitoringServiceClient>();

        return services;
    }

    public static IServiceCollection AddMonitoringServiceClient(
        this IServiceCollection services,
        string serviceUrl,
        Action<GrpcClientFactoryOptions> configureClient)
    {
        services.AddGrpcClient<Workshop.Contracts.Monitoring.MonitoringService.MonitoringServiceClient>(options =>
        {
            options.Address = new Uri(serviceUrl);
            configureClient(options);
        });

        services.AddScoped<MonitoringServiceClient>();

        return services;
    }
}
