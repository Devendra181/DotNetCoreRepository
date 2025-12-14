using Microsoft.Extensions.Options;
using OrderManagementAPI.Services;
namespace OrderManagementAPI.Logging
{
    // ILoggerProvider implementation responsible for creating TextFileLogger instances.
    // ASP.NET Core calls CreateLogger() once per logging "category" (e.g., "OrdersController",
    // "Microsoft.Hosting.Lifetime", etc.). Each category receives its own dedicated logger instance.
    public class TextFileLoggerProvider : ILoggerProvider
    {
        // Stores configuration options for this logger (directory path, filename pattern,
        // minimum log level, rolling file options, etc.).
        // IOptions allows these values to come from appsettings.json.
        private readonly TextFileLoggerOptions _options;

        // Allows the logger to access the current request's CorrelationId (if any).
        // Passing this to the logger enables writing correlation-aware log lines.
        private readonly ICorrelationIdAccessor _correlationIdAccessor;

        // Constructor executed during DI container initialization.
        // The provider receives logger configuration + correlationId accessor,
        // and reuses them for creating per-category loggers.
        public TextFileLoggerProvider(
            IOptions<TextFileLoggerOptions> options,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            // Bind options from configuration.
            _options = options.Value;

            // Store dependency for later use inside CreateLogger().
            _correlationIdAccessor = correlationIdAccessor;
        }

        // Called by ASP.NET Core internally whenever a logger is requested for a specific category.
        // Each category name results in a *new* TextFileLogger instance.
        // Example category names:
        //   - "OrderService"
        //   - "OrderManagementAPI.Controllers.OrdersController"
        // The returned logger handles writing logs to text files.
        public ILogger CreateLogger(string categoryName)
        {
            // Provide the logger with:
            //   • its categoryName (for identifying log origin)
            //   • configuration options (_options)
            //   • correlation ID accessor for writing request context
            return new TextFileLogger(categoryName, _options, _correlationIdAccessor);
        }

        // Called by ASP.NET Core during application shutdown.
        // Since this provider does not hold unmanaged resources,
        // there is nothing to clean up at the provider level.
        public void Dispose()
        {
            // No cleanup required at the moment.
        }
    }
}

