using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sales.Application.Exceptions;
using Sales.Domain.Common;

namespace Sales.Api.Errors;

/// <summary>
/// Single boundary translator from exceptions to HTTP responses. It guarantees
/// that an internal exception — a domain invariant violation, a failed lookup, a
/// validation error — leaves the service as a clean RFC 7807 <c>ProblemDetails</c>
/// payload with the correct status code, never as a leaked stack trace.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = Map(exception);
        problem.Instance = httpContext.Request.Path;

        var statusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled error processing {Path}.", httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning("Request to {Path} rejected ({Status}): {Detail}",
                httpContext.Request.Path, statusCode, problem.Detail ?? problem.Title);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            problem,
            problem.GetType(),
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }

    private static ProblemDetails Map(Exception exception) => exception switch
    {
        // FluentValidation failure raised by the Application ValidationBehavior.
        ValidationException validation => new ValidationProblemDetails(
            validation.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
        },

        // The use case referenced a cart that does not exist.
        CartNotFoundException notFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found.",
            Detail = notFound.Message,
        },

        // A domain invariant / state rule was violated.
        DomainException domain => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "The request conflicts with the current state of the cart.",
            Detail = domain.Message,
        },

        // Anything else — no detail is exposed to the caller.
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
        },
    };
}
