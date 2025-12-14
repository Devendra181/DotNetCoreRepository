using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
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

            // Register DbContext using SQL Server provider
            builder.Services.AddDbContext<OrderManagementDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Allow services to access HttpContext (used for CorrelationId, etc.)
            builder.Services.AddHttpContextAccessor();

            // Register application services
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();

            // Logging Configuration
            // Remove default logging providers
            builder.Logging.ClearProviders();

            // Log output to terminal or command prompt
            builder.Logging.AddConsole();

            // Log output to Visual Studio Debug window
            builder.Logging.AddDebug();

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

