using System.Text.Json.Serialization;
using EventBooking.API.Infrastructure;
using EventBooking.API.Middleware;
using EventBooking.Core.Configuration;
using EventBooking.Data;
using EventBooking.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;

// NLog is set up before the host so failures during startup still get logged.
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("EventBooking.API starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Route all Microsoft.Extensions.Logging (ILogger<T>) output through NLog.
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.

    builder.Services
        .AddControllers()
        .AddJsonOptions(o =>
            // Serialize/accept enums as their names ("Morning", "Confirmed") instead of numbers.
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(o =>
    {
        // "Authorize" button in Swagger UI: paste a JWT from /api/auth/login to call protected endpoints.
        o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the raw JWT (Swagger adds the 'Bearer ' prefix)."
        });
        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // Data layer: AppDbContext (Npgsql). Connection string comes from configuration / User Secrets.
    builder.Services.AddDataLayer(builder.Configuration);

    // Service layer: AutoMapper profiles, business services, auth (hasher + token service).
    builder.Services.AddServiceLayer(builder.Configuration);

    // --- Configuration: strongly-typed, validated on startup ---
    // Missing Issuer/Audience/Key (e.g. Jwt:Key not set in User Secrets) fails the host at
    // startup with a clear OptionsValidationException instead of a 500 on the first request.
    builder.Services
        .AddOptions<JwtOptions>()
        .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // --- Authentication: JWT bearer, configured from JwtOptions (see ConfigureJwtBearerOptions). ---
    builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer();

    builder.Services.AddAuthorization();

    // CORS: the React dev client (Vite) runs on a different origin and needs explicit permission.
    const string devClientCors = "DevClient";
    var clientOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? new[] { "http://localhost:5173" };
    builder.Services.AddCors(options => options.AddPolicy(devClientCors, policy =>
        policy.WithOrigins(clientOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

    var app = builder.Build();

    // --- HTTP pipeline, in order ---

    // 1. Error handling first: it must wrap everything below it.
    app.UseExceptionHandling();

    // 2. Correlation id: assigned before anything logs.
    app.UseCorrelationId();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // 3. CORS before auth, so preflight (OPTIONS) requests are answered without a token.
    app.UseCors(devClientCors);

    // 4. Authentication before authorization.
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Seed the demo accounts (needs the password hasher, so it cannot live in a migration).
    await app.SeedDemoUsersAsync();

    app.Run();
}
catch (HostAbortedException)
{
    // Thrown by EF Core design-time tools (`dotnet ef migrations`, `database update`).
    // It is the normal way those tools stop the host — not a failure.
    throw;
}
catch (Exception ex)
{
    logger.Error(ex, "EventBooking.API terminated unexpectedly during startup");
    throw;
}
finally
{
    // Flush and release file handles on shutdown.
    LogManager.Shutdown();
}
