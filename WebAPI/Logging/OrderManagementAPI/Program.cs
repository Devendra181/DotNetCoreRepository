using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Logging;
using OrderManagementAPI.Middlewares;
using OrderManagementAPI.Services;
namespace OrderManagementAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add controller support and configure JSON serialization
            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            // Enable Swagger for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register DbContext for main app usage
            builder.Services.AddDbContext<OrderManagementDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Allow services to access HttpContext (used for CorrelationId, etc.)
            builder.Services.AddHttpContextAccessor();

            // Register application services
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();

            // Bind custom logger options from configuration
            builder.Services.Configure<TextFileLoggerOptions>(
                builder.Configuration.GetSection("Logging:File"));

            builder.Services.Configure<DatabaseLoggerOptions>(
                builder.Configuration.GetSection("Logging:Database"));

            // Clear default providers and re-add Console + Debug
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Register custom providers via DI so the logging system picks them up
            builder.Services.AddSingleton<ILoggerProvider, TextFileLoggerProvider>();
            builder.Services.AddSingleton<ILoggerProvider, DatabaseLoggerProvider>();

            // Build the Application
            var app = builder.Build();

            // Middleware Pipeline Configuration

            // Enable Swagger UI (only in Development environment)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enforce HTTPS for all requests
            app.UseHttpsRedirection();

            // Attach a Correlation ID to each request for tracing
            app.UseCorrelationId();

            // Handle Authorization if enabled
            app.UseAuthorization();

            // Map controller endpoints to routes
            app.MapControllers();

            // Start the application
            app.Run();
        }
    }
}

