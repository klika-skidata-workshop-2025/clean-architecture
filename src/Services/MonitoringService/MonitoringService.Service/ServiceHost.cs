using Microsoft.AspNetCore.Builder;
using MonitoringService.Infrastructure;

namespace MonitoringService.Service;

/// <summary>
/// Represents a running instance of the MonitoringService.
/// </summary>
public class ServiceHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    internal ServiceHost(WebApplication app)
    {
        _app = app;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _app.Services.EnsureDatabaseAsync();
        await _app.StartAsync(cancellationToken);
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await _app.Services.EnsureDatabaseAsync();
        await _app.RunAsync(url: null);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _app.StopAsync(cancellationToken);
    }

    public IEnumerable<string> Urls => _app.Urls;

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
    }
}
