using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Middleware;

/// <summary>
/// The outermost middleware. Anything that escapes the pipeline as an exception is logged at
/// <c>Error</c> and turned into a uniform <see cref="ProblemDetails"/> JSON response (HTTP 500)
/// carrying the request's correlation id — never a raw stack trace (except the detail text in
/// Development, to help while building).
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected mid-request — there is nobody left to answer.
            _logger.LogInformation("Request {Method} {Path} was cancelled by the client.",
                context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var id)
                ? id?.ToString()
                : context.TraceIdentifier;

            // This middleware sits outside CorrelationIdMiddleware's logging scope, so re-attach
            // the id here — otherwise the Error line would be the one log line without it.
            using var _ = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId ?? "-"
            });

            _logger.LogError(ex, "Unhandled exception for {Method} {Path}.",
                context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                // Too late to replace the response — let the server tear the connection down.
                _logger.LogWarning("Response for {Path} already started; cannot write an error body.",
                    context.Request.Path);
                throw;
            }

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Detail = _env.IsDevelopment() ? ex.ToString() : "See the server logs for details."
            };
            problem.Extensions["correlationId"] = correlationId;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
