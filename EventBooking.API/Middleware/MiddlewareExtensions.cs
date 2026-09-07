namespace EventBooking.API.Middleware;

/// <summary>Pipeline registration helpers, so <c>Program.cs</c> reads as an ordered list.</summary>
public static class MiddlewareExtensions
{
    /// <summary>Catches unhandled exceptions → uniform JSON. Register first (outermost).</summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();

    /// <summary>Assigns / echoes the correlation id and scopes it into the logs.</summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
