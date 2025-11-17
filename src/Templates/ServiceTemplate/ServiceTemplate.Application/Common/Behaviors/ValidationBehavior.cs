using FluentResults;
using FluentValidation;
using MediatR;

namespace ServiceTemplate.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates requests using FluentValidation.
/// Runs before the request handler executes.
/// </summary>
/// <typeparam name="TRequest">The type of request being validated</typeparam>
/// <typeparam name="TResponse">The type of response (must be Result or Result<T>)</typeparam>
/// <remarks>
/// This behavior automatically validates all CQRS commands and queries
/// before they reach the handler. If validation fails, it returns a Result.Fail
/// instead of executing the handler.
/// </remarks>
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
        // If no validators registered, skip validation
        if (!_validators.Any())
        {
            return await next();
        }

        // Run all validators
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If validation failed, return Result.Fail with errors
        if (failures.Any())
        {
            var errors = failures.Select(failure =>
                new Error($"Validation failed for '{failure.PropertyName}': {failure.ErrorMessage}")
                    .WithMetadata("ErrorCode", "VALIDATION_ERROR")
                    .WithMetadata("PropertyName", failure.PropertyName)
                    .WithMetadata("AttemptedValue", failure.AttemptedValue?.ToString() ?? "null")
            ).ToList();

            // Create a failed result
            var result = Result.Fail(errors.First()); // FluentResults doesn't support multiple errors in constructor
            foreach (var error in errors.Skip(1))
            {
                result.WithError(error);
            }

            return (TResponse)(object)result;
        }

        // Validation passed, proceed to handler
        return await next();
    }
}
