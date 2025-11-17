using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ServiceTemplate.Client;

/// <summary>
/// Client for ServiceTemplate gRPC service.
/// Provides a strongly-typed interface for calling ServiceTemplate APIs.
/// </summary>
public class ServiceTemplateClient
{
    private readonly ILogger<ServiceTemplateClient> _logger;

    /// <summary>
    /// Initializes a new instance of ServiceTemplateClient.
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public ServiceTemplateClient(ILogger<ServiceTemplateClient> logger)
    {
        _logger = logger;
    }

    // TODO: Add client methods here
    // Example:
    // private readonly YourService.YourServiceClient _client;
    //
    // public async Task<YourResponse> YourMethodAsync(YourRequest request, CancellationToken cancellationToken = default)
    // {
    //     try
    //     {
    //         _logger.LogDebug("Calling YourMethod with request: {@Request}", request);
    //         var response = await _client.YourMethodAsync(request, cancellationToken: cancellationToken);
    //         _logger.LogDebug("YourMethod returned: {@Response}", response);
    //         return response;
    //     }
    //     catch (RpcException ex)
    //     {
    //         _logger.LogError(ex, "gRPC error calling YourMethod: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error calling YourMethod");
    //         throw;
    //     }
    // }
}
