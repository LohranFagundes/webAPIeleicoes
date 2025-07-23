using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using Serilog;
using ElectionApi.Net.Data;
using ElectionApi.Net.Services;
using ElectionApi.Net.Middleware;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure configuration to expand environment variables
builder.Configuration.AddEnvironmentVariables();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/election-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Database Configuration
var connectionString = BuildConnectionString();
builder.Services.AddDbContext<ElectionDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), 
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IElectionService, ElectionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IVoterService, VoterService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVotingService, VotingService>();
builder.Services.AddScoped<IVoteCryptographyService, VoteCryptographyService>();
builder.Services.AddScoped<ISecureVoteRepository, SecureVoteRepository>();
builder.Services.AddScoped<IVoteCountingService, VoteCountingService>();

// AutoMapper - commented out due to package conflicts
// builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// FluentValidation
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// JWT Authentication
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? builder.Configuration["JwtSettings:SecretKey"]!;
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["JwtSettings:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Election API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for easier testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Election API v1");
    c.RoutePrefix = "swagger"; // Changed from "docs" to "swagger" for standard access
});

// Custom Middleware
app.UseMiddleware<CorsMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();
app.UseMiddleware<SwaggerAuthMiddleware>();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Static Files (serve uploaded images)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// Redirect /docs to /swagger for backward compatibility
app.MapGet("/docs", () => Results.Redirect("/swagger"));

// API Info endpoint
app.MapGet("/", () => Results.Ok(new 
{ 
    Name = "Election API .NET", 
    Version = "1.0.0", 
    Environment = app.Environment.EnvironmentName,
    Documentation = "/swagger",
    Endpoints = new
    {
        Swagger = "/swagger",
        Health = "/health",
        AuthAdmin = "/api/auth/admin/login",
        AuthVoter = "/api/auth/voter/login",
        Elections = "/api/election"
    }
}));

try
{
    Log.Information("Starting Election API");
    
    // Seed the database in the background
    _ = Task.Run(async () =>
    {
        try
        {
            // Wait for app to start
            await Task.Delay(2000);
            
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ElectionDbContext>();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                await DataSeeder.SeedAsync(context, authService);
                Log.Information("Database seeding completed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during database seeding");
        }
    });
    
    // Start the application (this blocks until shutdown)
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Helper function to build connection string from environment variables
static string BuildConnectionString()
{
    var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
    var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
    var database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "election_system";
    var username = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "root";
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

    return $"Server={host};Port={port};Database={database};User={username};Password={password};CharSet=utf8mb4;";
}
