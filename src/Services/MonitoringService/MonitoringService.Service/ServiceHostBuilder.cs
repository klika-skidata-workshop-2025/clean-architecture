using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonitoringService.API.Services;
using MonitoringService.Application;
using MonitoringService.Infrastructure;

namespace MonitoringService.Service;

/// <summary>
/// Builder for creating and configuring the MonitoringService host.
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

    public static ServiceHostBuilder Create(string[] args) => new(args);

    public ServiceHostBuilder ConfigureWindowsService()
    {
        _builder.Host.UseWindowsService(options =>
        {
            options.ServiceName = "MonitoringService";
        });
        return this;
    }

    public ServiceHostBuilder ConfigureSystemd()
    {
        _builder.Host.UseSystemd();
        return this;
    }

    public ServiceHostBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        _configureServices = configure;
        return this;
    }

    public ServiceHostBuilder ConfigureApplication(Action<WebApplication> configure)
    {
        _configureApp = configure;
        return this;
    }

    public ServiceHost Build()
    {
        _builder.Services.AddApplicationServices();
        _builder.Services.AddInfrastructureServices(_builder.Configuration);

        _builder.Services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = _builder.Environment.IsDevelopment();
        });

        _builder.Services.AddGrpcReflection();

        _builder.Services.AddGrpcHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

        _configureServices?.Invoke(_builder.Services);

        var app = _builder.Build();

        app.MapGrpcService<MonitoringGrpcService>();
        app.MapGrpcHealthChecksService();

        if (app.Environment.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }

        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

        _configureApp?.Invoke(app);

        return new ServiceHost(app);
    }
}
