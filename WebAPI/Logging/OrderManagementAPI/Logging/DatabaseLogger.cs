using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Entities;
using OrderManagementAPI.Services;
namespace OrderManagementAPI.Logging
{
    // Custom logger responsible for writing log entries to the LogEntries table in SQL Server.
    // This class implements ILogger and is created by DatabaseLoggerProvider.
    public class DatabaseLogger : ILogger
    {
        // Category name (e.g., "OrderService", "OrdersController")
        // Usually the full class name including the namespace
        // Helps identify which component generated the log.
        private readonly string _categoryName;

        // Options that control how this logger behaves (e.g., minimum log level).
        private readonly DatabaseLoggerOptions _options;

        // Used to create a new DI scope for each logging operation.
        // From that scope, we resolve a scoped OrderManagementDbContext safely.
        private readonly IServiceScopeFactory _scopeFactory;

        // Helper service used to read the CorrelationId for the current request.
        private readonly ICorrelationIdAccessor _correlationIdAccessor;

        public DatabaseLogger(
            string categoryName,
            DatabaseLoggerOptions options,
            IServiceScopeFactory scopeFactory,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            _categoryName = categoryName;
            _options = options;
            _scopeFactory = scopeFactory;
            _correlationIdAccessor = correlationIdAccessor;
        }

        // We are not using scopes in this custom logger
        public IDisposable? BeginScope<TState>(TState state) => default;

        // Checks whether the given log level is enabled for this logger.
        // This allows us to skip building and writing log messages that are below the configured threshold
        // or when level is LogLevel.None.
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _options.MinimumLogLevel &&
                   logLevel != LogLevel.None;
        }

        // Main logging method called by the ASP.NET Core logging infrastructure.
        // It will be invoked for every log message in this category.
        // It formats the log message, checks if logging is enabled,
        // and sends the message for asynchronous DB persistence.
        public void Log<TState>(
            LogLevel logLevel,                          // The severity of the log (Information, Warning, Error, etc.)
            EventId eventId,                            // Optional identifier for the specific event being logged
            TState state,                               // The logging state (usually the message template and its data)
            Exception? exception,                       // Optional exception associated with this log entry (if any)
            Func<TState, Exception?, string> formatter) // Function that converts state + exception into the final message string
        {
            // If this log level is not enabled, do nothing.
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            // Build the final log message string using the provided formatter.
            var message = formatter(state, exception);

            // If there is no message and no exception, there is nothing to log.
            if (string.IsNullOrWhiteSpace(message) && exception == null)
                return;

            // Retrieve the current CorrelationId (if any) using the shared helper.
            var correlationId = _correlationIdAccessor.GetCorrelationId();

            // Write the log entry to the database asynchronously.
            // We fire-and-forget this task so that logging does not block the main request pipeline.
            _ = WriteLogAsync(logLevel, message, exception, correlationId);
        }

        // Performs the actual database write using a fresh DbContext instance.
        // This method is async and is intentionally not awaited in Log(), to keep logging non-blocking.
        private async Task WriteLogAsync(LogLevel logLevel, string message, Exception? exception, string? correlationId)
        {
            try
            {
                // Create a new service scope so we can resolve scoped services (like DbContext)
                // from within this (logger) singleton.
                using var scope = _scopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();

                // Map the log information to our LogEntry entity.
                var logEntry = new LogEntry
                {
                    TimestampUtc = DateTime.UtcNow,
                    LogLevel = logLevel.ToString(),
                    Category = _categoryName,
                    Message = message,
                    ExceptionMessage = exception?.Message,
                    ExceptionStackTrace = exception?.StackTrace,
                    CorrelationId = correlationId
                };

                // Insert the log entry into the LogEntries table.
                await dbContext.LogEntries.AddAsync(logEntry);

                // Persist changes to the database.
                await dbContext.SaveChangesAsync();
            }
            catch
            {
                // IMPORTANT:
                // Never throw from a logger. If logging fails (e.g., DB down),
                // we ignore the exception to avoid breaking the main application flow.
            }
        }
    }
}
