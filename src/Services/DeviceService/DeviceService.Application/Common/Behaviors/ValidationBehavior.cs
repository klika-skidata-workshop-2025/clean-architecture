using FluentResults;
using FluentValidation;
using MediatR;

namespace DeviceService.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates requests using FluentValidation.
/// Automatically validates all requests that have validators registered.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type (must be ResultBase)</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : ResultBase
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators are registered, skip validation
        if (!_validators.Any())
        {
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators in parallel
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        // If there are validation errors, return a failed result
        if (failures.Any())
        {
            var errors = failures
                .Select(failure => new Error($"{failure.PropertyName}: {failure.ErrorMessage}")
                    .WithMetadata("ErrorCode", "VALIDATION_ERROR")
                    .WithMetadata("PropertyName", failure.PropertyName))
                .ToList();

            // Return the first error (you can customize this to return all errors)
            return (TResponse)(object)Result.Fail(errors.First());
        }

        // Validation passed, continue with the request
        return await next();
    }
}
