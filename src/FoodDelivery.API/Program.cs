using System.Text;
using FoodDelivery.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using dotenv.net;

// Load environment variables from .env file
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Override with environment variables if present
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

if (!string.IsNullOrEmpty(dbHost))
{
    connectionString = $"Host={dbHost};Port={dbPort ?? "5432"};Database={dbName};Username={dbUser};Password={dbPassword}";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
                builder.Configuration["Jwt:Secret"] ?? 
                "YourSuperSecretKeyHere12345678901234567890";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Food Delivery API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI to serve at the root
    });
}

// app.UseHttpsRedirection(); // Disabled for development
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Initialize database and seed data on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Try to ensure database exists and apply migrations
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        // If migrations fail (e.g., tables already exist via SQL script), just log and continue
        logger.LogWarning("Migration skipped: {Message}. If you created tables via SQL script, this is expected.", ex.Message);
        
        // Ensure database connection is valid
        if (!await db.Database.CanConnectAsync())
        {
            logger.LogError("Cannot connect to database. Please check your connection string.");
            throw;
        }
    }
    
    // Seed data if database is empty
    try
    {
        await DbSeeder.SeedAsync(db);
        logger.LogInformation("Database seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogWarning("Seeding skipped: {Message}", ex.Message);
    }
}

await app.RunAsync();
