using Asp.Versioning;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using ServerMonitoring.API.HealthChecks;
using ServerMonitoring.API.Middleware;
using ServerMonitoring.API.Hubs;
using System.Text;
using ServerMonitoring.Application;
using ServerMonitoring.Infrastructure;
using ServerMonitoring.Infrastructure.Data;
using ServerMonitoring.Infrastructure.Interceptors;
using ServerMonitoring.Infrastructure.BackgroundJobs;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Database
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase") || 
                          Environment.GetEnvironmentVariable("UseInMemoryDatabase") == "true";

builder.Services.AddScoped<AuditInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    if (useInMemoryDatabase)
    {
        options.UseInMemoryDatabase("ServerMonitoringTestDb");
        Log.Information("Using In-Memory Database for testing");
    }
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlite(connectionString);
        Log.Information("Using SQLite Database: {ConnectionString}", connectionString);
    }
});

builder.Services.AddScoped<ServerMonitoring.Application.Interfaces.IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());

// Application layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// JWT Auth
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // SignalR authentication
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Server Monitoring API", 
        Version = "v1",
        Description = "Enterprise Server Monitoring System with Hangfire & SignalR"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token"
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
});

builder.Services.AddSignalR();

// Response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Hangfire - Use SQLite storage in app directory
var hangfireDbPath = Path.Combine(AppContext.BaseDirectory, "hangfire.db");
// Ensure directory exists
Directory.CreateDirectory(Path.GetDirectoryName(hangfireDbPath)!);
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(hangfireDbPath));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
});

builder.Services.AddScoped<MetricsCollectionJob>();
builder.Services.AddScoped<AlertProcessingJob>();
builder.Services.AddScoped<ReportGenerationJob>();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db" });

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc();

// CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000", "https://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<CorrelationIdMiddleware>();
builder.Services.AddScoped<IdempotencyMiddleware>();
builder.Services.AddScoped<GlobalExceptionHandlerMiddleware>();
builder.Services.AddScoped<RequestResponseLoggingMiddleware>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var authService = scope.ServiceProvider.GetRequiredService<ServerMonitoring.Application.Interfaces.IAuthService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        if (useInMemoryDatabase)
        {
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("In-memory database created");
        }
        else
        {
            // For SQLite, ensure database and schema are created
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("SQLite database created/verified");
        }
        
        await ServerMonitoring.Infrastructure.Seeders.DatabaseSeeder.SeedAsync(context, authService);
        logger.LogInformation("Database seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize database");
        throw;
    }
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseResponseCompression();

app.UseCors("DefaultCorsPolicy");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Server Monitoring API v1");
    options.RoutePrefix = "swagger";
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
app.UseRateLimiting();

app.UseStaticFiles(); // Enable static files for Hangfire dashboard

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard - accessible without authentication for demo
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllDashboardAuthorizationFilter() },
    DashboardTitle = "Server Monitoring - Hangfire Dashboard",
    AppPath = null, // Disable back button
    StatsPollingInterval = 2000,
    IgnoreAntiforgeryToken = true,
    DisplayStorageConnectionString = false
});

app.MapHub<MonitoringHub>("/hubs/monitoring");
app.MapHealthChecks("/health");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        RecurringJob.AddOrUpdate<MetricsCollectionJob>(
            "collect-metrics",
            job => job.CollectMetricsAsync(),
            "*/5 * * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        RecurringJob.AddOrUpdate<AlertProcessingJob>(
            "process-alerts",
            job => job.ProcessAlertsAsync(),
            "*/2 * * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        logger.LogInformation("Hangfire recurring jobs configured successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to configure Hangfire recurring jobs");
    }
}

app.Logger.LogInformation("Server Monitoring API Started");
app.Logger.LogInformation("Swagger UI: /swagger");
app.Logger.LogInformation("Hangfire Dashboard: /hangfire");
app.Logger.LogInformation("SignalR Hub: /hubs/monitoring");
app.Logger.LogInformation("Health Check: /health");

app.Run();

// Hangfire Dashboard Authorization - allows all in demo environment
public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true; // Allow all requests for demo/assessment
    }
}

// Make Program class accessible to test projects
public partial class Program { }
