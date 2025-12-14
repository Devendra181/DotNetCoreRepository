using Microsoft.Extensions.Options;
using OrderManagementAPI.Services;
namespace OrderManagementAPI.Logging
{
    // The DatabaseLoggerProvider is responsible for creating DatabaseLogger instances.
    // ASP.NET Core calls CreateLogger() once per logging category (e.g., "OrderService"),
    // and the returned logger is reused throughout the application's lifetime.
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        // Holds the configuration options for the database logger (MinimumLogLevel, etc.).
        // Using IOptions allows configuration via appsettings.json.
        private readonly DatabaseLoggerOptions _options;

        // Used to create a new DI scope for each logging operation.
        // From that scope, we resolve a scoped OrderManagementDbContext safely.
        private readonly IServiceScopeFactory _scopeFactory;

        // Provides access to the current request's CorrelationId.
        // This allows each database log entry to be associated with the originating request.
        private readonly ICorrelationIdAccessor _correlationIdAccessor;

        // The provider constructor receives injected dependencies and configuration options.
        // These objects are reused when creating loggers for each category.
        public DatabaseLoggerProvider(
            IOptions<DatabaseLoggerOptions> options,
            IServiceScopeFactory scopeFactory,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            // Read options from configuration binding (e.g., appsettings.json)
            _options = options.Value;

            _scopeFactory = scopeFactory;

            // CorrelationId accessor is passed down to each logger
            _correlationIdAccessor = correlationIdAccessor;
        }

        // Called by ASP.NET Core whenever it needs an ILogger for a given category.
        // Example categories: "OrderService", "Microsoft.AspNetCore.Hosting", etc.
        // This method must return a NEW DatabaseLogger instance for each category.
        public ILogger CreateLogger(string categoryName)
        {
            // Create a DatabaseLogger tailored to the given logging category.
            // The provider supplies shared dependencies (options, _scopeFactory, CorrelationIdAccessor).
            return new DatabaseLogger(categoryName, _options, _scopeFactory, _correlationIdAccessor);
        }

        // Called by ASP.NET Core during application shutdown.
        // Since this provider holds no unmanaged resources, nothing needs disposal.
        public void Dispose()
        {
            // No unmanaged resources to clean up. Method exists to satisfy ILoggerProvider contract.
        }
    }
}
