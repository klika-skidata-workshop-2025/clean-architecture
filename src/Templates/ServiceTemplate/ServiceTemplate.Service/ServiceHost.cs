using Microsoft.AspNetCore.Builder;
using ServiceTemplate.Infrastructure;

namespace ServiceTemplate.Service;

/// <summary>
/// Represents a running instance of the ServiceTemplate service.
/// Provides methods to start, stop, and manage the service lifecycle.
/// </summary>
public class ServiceHost : IAsyncDisposable
{
    private readonly WebApplication _app;
    private CancellationTokenSource? _cts;

    internal ServiceHost(WebApplication app)
    {
        _app = app;
    }

    /// <summary>
    /// Starts the service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when the service is started</returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // Ensure database is created and migrated
        await _app.Services.EnsureDatabaseAsync();

        // Start the application
        await _app.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Runs the service and blocks until it is shut down.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when the service is stopped</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        // Ensure database is created and migrated
        await _app.Services.EnsureDatabaseAsync();

        // Run the application (use url: null to use configured URLs)
        await _app.RunAsync(url: null);
    }

    /// <summary>
    /// Stops the service gracefully.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task that completes when the service is stopped</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _app.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the service's URL(s).
    /// </summary>
    public IEnumerable<string> Urls => _app.Urls;

    /// <summary>
    /// Disposes the service host.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        await _app.DisposeAsync();
    }
}
