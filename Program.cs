using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantBooking.Data;
using RestaurantBooking.Services;
using System.Text;
using System.Text.Json;
namespace RestaurantBooking
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Enhanced service configuration
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            builder.Services.AddEndpointsApiExplorer();

            // Production-grade Swagger configuration
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title = "Restaurant Booking Management API",
                    Version = "v1",
                    Description = "Comprehensive restaurant table booking and management system",
                    Contact = new() { Name = "Restaurant API", Email = "api@restaurant.com" }
                });

                // JWT Security Definition
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
            });

            // Database configuration with enhanced error handling
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<RestaurantContext>(options =>
            {
                options.UseSqlServer(connectionString);
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Dependency injection
            builder.Services.AddScoped<IBookingService, BookingService>();

            // JWT Authentication with comprehensive validation
            var jwtConfig = builder.Configuration.GetSection("Jwt");
            var secretKey = jwtConfig["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("RestaurantApiPolicy", policy =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    }
                    else
                    {
                        policy.WithOrigins("https://yourdomain.com")
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    }
                });
            });

            var app = builder.Build();

            // Enhanced middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant Booking API V1");
                    c.RoutePrefix = "swagger";
                    c.DefaultModelsExpandDepth(-1);
                });
            }

            // Root endpoint with API discovery
            app.MapGet("/", () => Results.Json(new
            {
                service = "Restaurant Booking Management API",
                version = "1.0.0",
                status = "operational",
                timestamp = DateTime.UtcNow,
                endpoints = new
                {
                    documentation = "/swagger",
                    authentication = "/api/auth/login",
                    menu = "/api/menu",
                    availableTables = "/api/bookings/available-tables",
                    health = "/health"
                },
                authentication = new
                {
                    type = "JWT Bearer Token",
                    defaultCredentials = new { username = "admin", password = "admin123" }
                }
            }));

            // Health check endpoint
            app.MapGet("/health", async (RestaurantContext context) =>
            {
                try
                {
                    await context.Database.CanConnectAsync();
                    return Results.Json(new
                    {
                        status = "healthy",
                        database = "connected",
                        timestamp = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    return Results.Json(new
                    {
                        status = "unhealthy",
                        database = "disconnected",
                        error = ex.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            });

            // API status endpoint
            app.MapGet("/api", () => Results.Json(new
            {
                message = "Restaurant Booking API is running",
                availableEndpoints = new[]
                {
        "GET /api/menu - View restaurant menu",
        "GET /api/bookings/available-tables - Check table availability",
        "POST /api/auth/login - Admin authentication",
        "GET /swagger - API documentation"
    }
            }));

            app.UseHttpsRedirection();
            app.UseCors("RestaurantApiPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Database initialization with comprehensive error handling
            await InitializeDatabaseAsync(app);

            async Task InitializeDatabaseAsync(WebApplication application)
            {
                using var scope = application.Services.CreateScope();
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var context = services.GetRequiredService<RestaurantContext>();
                    logger.LogInformation("Initializing database...");

                    await DbInitializer.InitializeAsync(context);

                    var tablesCount = await context.Tables.CountAsync();
                    var menuItemsCount = await context.MenuItems.CountAsync();

                    logger.LogInformation("Database initialized successfully. Tables: {TablesCount}, Menu Items: {MenuItemsCount}",
                        tablesCount, menuItemsCount);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Database initialization failed: {Error}", ex.Message);

                    if (!application.Environment.IsDevelopment())
                    {
                        throw new InvalidOperationException("Database initialization failed in production environment", ex);
                    }

                    logger.LogWarning("Continuing in development mode despite database initialization failure");
                }
            }

            app.Run();
        }
    }
}
