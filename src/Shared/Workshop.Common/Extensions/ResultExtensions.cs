using FluentResults;
using Grpc.Core;

namespace Workshop.Common.Extensions;

/// <summary>
/// Extension methods for FluentResults to integrate with gRPC services.
/// These methods provide a clean way to convert Result objects into gRPC responses and exceptions.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a failed Result into an RpcException with appropriate status code.
    /// This method maps common error types to gRPC status codes for proper client handling.
    /// </summary>
    /// <param name="result">The failed Result object</param>
    /// <returns>An RpcException with mapped status code and error details</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to convert a successful result to an exception</exception>
    /// <example>
    /// <code>
    /// var result = Result.Fail("Device not found");
    /// throw result.ToRpcException();
    /// </code>
    /// </example>
    public static RpcException ToRpcException(this ResultBase result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result to an RpcException");
        }

        // Get the first error (FluentResults supports multiple errors)
        var firstError = result.Errors.FirstOrDefault();
        if (firstError == null)
        {
            return new RpcException(new Status(StatusCode.Unknown, "An unknown error occurred"));
        }

        // Extract error code from metadata (if available)
        var errorCode = firstError.Metadata.GetValueOrDefault("ErrorCode")?.ToString() ?? "UNKNOWN_ERROR";

        // Map error codes to gRPC status codes
        // This provides semantic meaning to clients about what went wrong
        var statusCode = errorCode switch
        {
            var code when code.Contains("NOT_FOUND") => StatusCode.NotFound,
            var code when code.Contains("INVALID") => StatusCode.InvalidArgument,
            var code when code.Contains("ALREADY_EXISTS") => StatusCode.AlreadyExists,
            var code when code.Contains("CONFLICT") => StatusCode.AlreadyExists,
            var code when code.Contains("UNAUTHORIZED") => StatusCode.Unauthenticated,
            var code when code.Contains("FORBIDDEN") => StatusCode.PermissionDenied,
            var code when code.Contains("UNAVAILABLE") => StatusCode.Unavailable,
            _ => StatusCode.Internal
        };

        // Build gRPC metadata with error details
        var metadata = new Metadata();
        foreach (var (key, value) in firstError.Metadata)
        {
            metadata.Add(key, value?.ToString() ?? string.Empty);
        }

        return new RpcException(new Status(statusCode, firstError.Message), metadata);
    }

    /// <summary>
    /// Matches a Result asynchronously and transforms it to a response type.
    /// This is a railway-oriented programming approach that handles both success and failure paths.
    /// </summary>
    /// <typeparam name="T">The type of the successful result value</typeparam>
    /// <typeparam name="TResponse">The type of the response to return</typeparam>
    /// <param name="resultTask">The async Result to match on</param>
    /// <param name="onSuccess">Function to execute on success path (transforms value to response)</param>
    /// <param name="onFailure">Function to execute on failure path (transforms error to exception)</param>
    /// <returns>The transformed response if successful</returns>
    /// <exception cref="RpcException">Thrown if the result is a failure</exception>
    /// <example>
    /// <code>
    /// return await _mediator.Send(query).MatchAsync(
    ///     dto => new DeviceStatusResponse { DeviceId = dto.DeviceId },
    ///     error => error.ToRpcException()
    /// );
    /// </code>
    /// </example>
    public static async Task<TResponse> MatchAsync<T, TResponse>(
        this Task<Result<T>> resultTask,
        Func<T, TResponse> onSuccess,
        Func<ResultBase, RpcException> onFailure)
    {
        var result = await resultTask;

        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        throw onFailure(result);
    }

    /// <summary>
    /// Matches a Result and transforms it to a response type (synchronous version).
    /// </summary>
    /// <typeparam name="T">The type of the successful result value</typeparam>
    /// <typeparam name="TResponse">The type of the response to return</typeparam>
    /// <param name="result">The Result to match on</param>
    /// <param name="onSuccess">Function to execute on success path</param>
    /// <param name="onFailure">Function to execute on failure path</param>
    /// <returns>The transformed response if successful</returns>
    /// <exception cref="RpcException">Thrown if the result is a failure</exception>
    public static TResponse Match<T, TResponse>(
        this Result<T> result,
        Func<T, TResponse> onSuccess,
        Func<ResultBase, RpcException> onFailure)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        throw onFailure(result);
    }

    /// <summary>
    /// Executes an action asynchronously if the result is successful, otherwise throws an RpcException.
    /// Useful for command handlers that don't return values.
    /// </summary>
    /// <param name="resultTask">The async Result to check</param>
    /// <exception cref="RpcException">Thrown if the result is a failure</exception>
    /// <example>
    /// <code>
    /// await _mediator.Send(command).ThrowIfFailureAsync();
    /// return new UpdateDeviceResponse { Success = true };
    /// </code>
    /// </example>
    public static async Task ThrowIfFailureAsync(this Task<Result> resultTask)
    {
        var result = await resultTask;
        if (result.IsFailed)
        {
            throw result.ToRpcException();
        }
    }

    /// <summary>
    /// Checks if a result is failed and throws an RpcException (synchronous version).
    /// </summary>
    /// <param name="result">The Result to check</param>
    /// <exception cref="RpcException">Thrown if the result is a failure</exception>
    public static void ThrowIfFailure(this Result result)
    {
        if (result.IsFailed)
        {
            throw result.ToRpcException();
        }
    }
}
