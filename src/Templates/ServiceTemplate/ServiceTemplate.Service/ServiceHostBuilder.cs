using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceTemplate.Application;
using ServiceTemplate.Infrastructure;

namespace ServiceTemplate.Service;

/// <summary>
/// Builder for creating and configuring the ServiceTemplate host.
/// This class provides a fluent API for setting up the service with all required dependencies.
/// </summary>
public class ServiceHostBuilder
{
    private readonly WebApplicationBuilder _builder;
    private Action<IServiceCollection>? _configureServices;
    private Action<WebApplication>? _configureApp;

    private ServiceHostBuilder(string[] args)
    {
        _builder = WebApplication.CreateBuilder(args);
    }

    /// <summary>
    /// Creates a new ServiceHostBuilder instance.
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>ServiceHostBuilder instance</returns>
    public static ServiceHostBuilder Create(string[] args)
    {
        return new ServiceHostBuilder(args);
    }

    /// <summary>
    /// Configures the service to run as a Windows Service.
    /// </summary>
    /// <returns>ServiceHostBuilder for chaining</returns>
    public ServiceHostBuilder ConfigureWindowsService()
    {
        _builder.Host.UseWindowsService(options =>
        {
            options.ServiceName = "ServiceTemplate";
        });
        return this;
    }

    /// <summary>
    /// Configures the service to run as a Linux systemd service.
    /// </summary>
    /// <returns>ServiceHostBuilder for chaining</returns>
    public ServiceHostBuilder ConfigureSystemd()
    {
        _builder.Host.UseSystemd();
        return this;
    }

    /// <summary>
    /// Adds additional service configuration.
    /// </summary>
    /// <param name="configure">Configuration action</param>
    /// <returns>ServiceHostBuilder for chaining</returns>
    public ServiceHostBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        _configureServices = configure;
        return this;
    }

    /// <summary>
    /// Adds additional application configuration.
    /// </summary>
    /// <param name="configure">Configuration action</param>
    /// <returns>ServiceHostBuilder for chaining</returns>
    public ServiceHostBuilder ConfigureApplication(Action<WebApplication> configure)
    {
        _configureApp = configure;
        return this;
    }

    /// <summary>
    /// Builds the service host.
    /// </summary>
    /// <returns>Configured ServiceHost instance</returns>
    public ServiceHost Build()
    {
        // Add core services
        _builder.Services.AddApplicationServices();
        _builder.Services.AddInfrastructureServices(_builder.Configuration);

        // gRPC services
        _builder.Services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = _builder.Environment.IsDevelopment();
        });

        // gRPC reflection (for tools like grpcurl)
        _builder.Services.AddGrpcReflection();

        // Health checks
        _builder.Services.AddGrpcHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

        // Apply custom service configuration
        _configureServices?.Invoke(_builder.Services);

        var app = _builder.Build();

        // Map health check service
        app.MapGrpcHealthChecksService();

        // Map gRPC reflection service (development only)
        if (app.Environment.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }

        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086032");

        // Apply custom app configuration
        _configureApp?.Invoke(app);

        return new ServiceHost(app);
    }
}
