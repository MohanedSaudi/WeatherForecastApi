using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using WeatherApi.API.Common.Extensions;
using WeatherApi.API.Common.Middleware;
using WeatherApi.Application;
using WeatherApi.Application.Common.Constants;
using WeatherApi.Domain.Enums;
using WeatherApi.Infrastructure;
using WeatherApi.Infrastructure.Common.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERILOG CONFIGURATION ====================
builder.Host.AddCustomSerilog(builder.Configuration);

// ==================== CONTROLLERS ====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ==================== SWAGGER/OPENAPI ====================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Weather Forecast API",
        Version = "v1.0",
        Description = @"
            A production-ready Clean Architecture Weather API featuring:
            - JWT Authentication with Refresh Tokens
            - Role-based Authorization (Admin, Premium, User)
            - SQL Server with EF Core
            - Result Pattern for error handling
            - CQRS with MediatR
            - Comprehensive security (CSRF, XSS, Rate Limiting)
            - Resilience patterns (Retry, Circuit Breaker)
            - Docker support
        ",
        Contact = new OpenApiContact
        {
            Name = "Weather API Team",
            Email = "support@weatherapi.com",
            Url = new Uri("https://github.com/yourusername/weather-api")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
           Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ==================== APPLICATION & INFRASTRUCTURE ====================
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ==================== ANTI-FORGERY (CSRF PROTECTION) ====================
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false; // JavaScript needs to read it
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// ==================== RATE LIMITING ====================
builder.Services.AddCustomRateLimiting();

// ==================== RESPONSE COMPRESSION ====================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>(); 
    options.Providers.Add<GzipCompressionProvider>();   
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/problem+json" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

// ==================== JWT AUTHENTICATION ====================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to get token from Authorization header first
                var token = context.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();

                // Fallback to cookie if header is not present
                if (string.IsNullOrEmpty(token))
                {
                    token = context.Request.Cookies["accessToken"];
                }

                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Log.Warning("Authentication failed for {Path}: {Error}",
                    context.Request.Path,
                    context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst("userId")?.Value;
                var username = context.Principal?.Identity?.Name;

                Log.Information("Token validated for user: {Username} (ID: {UserId})",
                    username ?? "Unknown",
                    userId ?? "Unknown");

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Log.Warning("Authentication challenge for {Path}", context.Request.Path);
                return Task.CompletedTask;
            }
        };
    });

// ==================== AUTHORIZATION POLICIES ====================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.RequireAdminRole, policy =>
        policy.RequireRole(UserRole.Admin.ToString()));

    options.AddPolicy(Policies.RequirePremiumRole, policy =>
        policy.RequireRole(UserRole.Premium.ToString(), UserRole.Admin.ToString()));

    options.AddPolicy(Policies.RequireUserRole, policy =>
        policy.RequireAuthenticatedUser());
});

// ==================== CORS ====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? new[] { "http://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for cookies
    });
});

// ==================== HEALTH CHECKS ====================
// Add Health Checks UI
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage(); // Stores health history in memory

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// ==================== BUILD APP ====================
var app = builder.Build();

// ==================== DATABASE INITIALIZATION ====================
using (var scope = app.Services.CreateScope())
{
    try
    {
        Log.Information("=== Starting Database Initialization ===");
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
        Log.Information("=== Database Initialization Completed Successfully ===");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "=== DATABASE INITIALIZATION FAILED ===");

        if (!app.Environment.IsDevelopment())
        {
            throw; // Fail fast in production
        }
    }
}

// ==================== MIDDLEWARE PIPELINE (ORDER MATTERS!) ====================

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API V1");
       // c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.EnableTryItOutByDefault();
    });

    app.UseDeveloperExceptionPage();
}
else
{
    // Production error page
    app.UseExceptionHandler("/error");
}

// Request logging with Serilog
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("userId")?.Value);
        }
    };

    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null) return Serilog.Events.LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 500) return Serilog.Events.LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 400) return Serilog.Events.LogEventLevel.Warning;
        if (elapsed > 1000) return Serilog.Events.LogEventLevel.Warning;
        return Serilog.Events.LogEventLevel.Information;
    };
});

// Security headers
 app.UseMiddleware<SecurityHeadersMiddleware>();

// Global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// Response compression
app.UseResponseCompression();

// HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
// CORS
app.UseCors("AllowSpecificOrigin");

// Rate limiting
app.UseRateLimiter();

// Anti-CSRF middleware
app.UseAntiforgery();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoint
// 1. API Endpoint (Formatted for the UI)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// 2. The Dashboard UI
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-dashboard"; // Access dashboard here
});

// Fallback error endpoint for production
app.MapGet("/error", () => Results.Problem("An error occurred."))
    .ExcludeFromDescription();

// ==================== STARTUP ====================
try
{
    Log.Information("╔══════════════════════════════════════════════════════════════╗");
    Log.Information("║          Weather Forecast API Starting...                    ║");
    Log.Information("╚══════════════════════════════════════════════════════════════╝");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Application Name: {ApplicationName}", builder.Environment.ApplicationName);
    Log.Information("Content Root: {ContentRoot}", builder.Environment.ContentRootPath);

    if (app.Environment.IsDevelopment())
    {
        Log.Information("Swagger UI: https://localhost:5001 or http://localhost:5000");
        Log.Information("Seq Logs: http://localhost:5341");
    }

    Log.Information("╔══════════════════════════════════════════════════════════════╗");
    Log.Information("║          Application Started Successfully                    ║");
    Log.Information("╚══════════════════════════════════════════════════════════════╝");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "╔══════════════════════════════════════════════════════════════╗");
    Log.Fatal(ex, "║          APPLICATION TERMINATED UNEXPECTEDLY                 ║");
    Log.Fatal(ex, "╚══════════════════════════════════════════════════════════════╝");
}
finally
{
    Log.Information("Shutting down application...");
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }