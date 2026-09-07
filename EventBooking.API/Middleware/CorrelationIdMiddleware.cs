namespace EventBooking.API.Middleware;

/// <summary>
/// Gives every request a correlation id: taken from the <c>X-Correlation-Id</c> request header
/// when the caller supplies one, otherwise freshly generated. The id is put on
/// <see cref="HttpContext.Items"/>, echoed back on the response header, and pushed into a logging
/// scope so every log line for this request carries it (NLog picks it up in Step K).
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers.TryGetValue(HeaderName, out var incoming) &&
            !string.IsNullOrWhiteSpace(incoming)
                ? incoming.ToString()
                : Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.TraceIdentifier = correlationId;

        // Headers must be set before the response body starts streaming.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            _logger.LogInformation("Incoming request {Method} {Path}", context.Request.Method, context.Request.Path);
            await _next(context);
        }
    }
}
