using FluentValidation;
using Identity.Application.Exceptions;
using Identity.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Errors;

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
            _logger.LogWarning(
                "Request to {Path} rejected ({Status}): {Detail}",
                httpContext.Request.Path,
                statusCode,
                problem.Detail ?? problem.Title);
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

        UserNotFoundException notFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found.",
            Detail = notFound.Message,
        },

        InvalidCredentialsException => new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication failed.",
            Detail = "The username or password is incorrect.",
        },

        DuplicateRegistrationException duplicate => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Registration conflict.",
            Detail = duplicate.Message,
        },

        DomainException domain => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "The request conflicts with identity rules.",
            Detail = domain.Message,
        },

        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
        },
    };
}
