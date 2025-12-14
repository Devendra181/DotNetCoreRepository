using OrderManagementAPI.Services;
using System.Text;
namespace OrderManagementAPI.Logging
{
    // Custom logger responsible for writing log messages to a text file.
    // One instance is created per logging category (e.g., OrdersController, OrderService).
    public class TextFileLogger : ILogger
    {
        // Category name (e.g., "OrderService", "OrdersController").
        // Helps identify which component generated the log entry in the log file.
        private readonly string _categoryName;

        // Options that control how the file logger behaves (e.g., file path, minimum log level).
        private readonly TextFileLoggerOptions _options;

        // Helper service used to read the CorrelationId for the current HTTP request.
        // This keeps CorrelationId logic in a single reusable place.
        private readonly ICorrelationIdAccessor _correlationIdAccessor;

        // A shared lock object to make file writes thread-safe across multiple parallel requests.
        private static readonly object _fileLock = new object();

        public TextFileLogger(
            string categoryName,
            TextFileLoggerOptions options,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            _categoryName = categoryName;
            _options = options;
            _correlationIdAccessor = correlationIdAccessor;
        }

        // This logger does not make use of logging scopes.
        // We simply return null (or a no-op disposable in more advanced implementations).
        public IDisposable? BeginScope<TState>(TState state) => default;

        // Indicates whether the given log level is enabled for this logger.
        // This allows us to skip building and writing log messages that are below the configured threshold.
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _options.MinimumLogLevel &&
                   logLevel != LogLevel.None;
        }

        // Main logging method called by the ASP.NET Core logging infrastructure.
        // It checks if the log level is enabled, builds the log message, and writes it to a text file.
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            // If this log level is disabled, do nothing.
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            // Build the final log message string using the provided formatter.
            // The formatter combines the message template and any structured data.
            var message = formatter(state, exception);

            // If there is nothing meaningful to log, skip writing.
            if (string.IsNullOrWhiteSpace(message) && exception == null)
                return;

            // Retrieve the current CorrelationId (if any) using the shared helper.
            // This helps in tracing the same request across multiple logs.
            var correlationId = _correlationIdAccessor.GetCorrelationId();

            // Build a single formatted line to be written to the log file.
            var logLine = BuildLogLine(logLevel, eventId, message, exception, correlationId);

            try
            {
                // Decide which file path to use based on the configuration:
                // - UseDailyRollingFiles = true  → one file per day (filename-YYYY-MM-DD.ext)
                // - UseDailyRollingFiles = false → single file specified by FilePath
                var logFilePath = _options.UseDailyRollingFiles
                    ? GetDailyFilePath()
                    : _options.FilePath;

                // Ensure the target directory exists before writing the log file.
                var directory = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Thread-safe append to the log file so concurrent requests do not corrupt the file.
                lock (_fileLock)
                {
                    File.AppendAllText(logFilePath, logLine, Encoding.UTF8);
                }
            }
            catch
            {
                // IMPORTANT:
                // Never throw from a logger. If file logging fails (e.g., disk full),
                // we swallow the exception to avoid impacting the main application logic.
            }
        }

        // Builds a formatted log line that will be written to the text file.
        // Includes:
        //   - Timestamp (UTC)
        //   - Log level
        //   - Category
        //   - CorrelationId (if present)
        //   - Message
        //   - Exception message and stack trace (if present)
        private string BuildLogLine(
            LogLevel logLevel,
            EventId eventId,
            string message,
            Exception? exception,
            string? correlationId)
        {
            var sb = new StringBuilder();

            // Timestamp in UTC for easier correlation across servers and services.
            sb.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append(" [");
            sb.Append(logLevel.ToString().ToUpper());
            sb.Append("] ");

            // Category helps identify which component generated this log.
            sb.Append(_categoryName);

            // Optional: include CorrelationId if available.
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                sb.Append(" [CorrelationId: ");
                sb.Append(correlationId);
                sb.Append(']');
            }

            sb.Append(" - ");
            sb.Append(message);

            // If an exception is present, append its message and stack trace for easier debugging.
            if (exception != null)
            {
                sb.Append(" | Exception: ");
                sb.Append(exception.Message);
                sb.Append(" | StackTrace: ");
                sb.Append(exception.StackTrace);
            }

            sb.AppendLine();

            return sb.ToString();
        }

        // Builds the full log file path for the current day by appending the date to the base file name.
        //
        // Example:
        //   Base FilePath: "Logs/order-api-log.txt"
        //   Today (UTC):   2025-12-10
        //   Result:        "Logs/order-api-log-2025-12-10.txt"
        private string GetDailyFilePath()
        {
            var baseFilePath = _options.FilePath;

            var directory = Path.GetDirectoryName(baseFilePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseFilePath);
            var extension = Path.GetExtension(baseFilePath);

            // Use UTC date for consistency; use DateTime.Now if you prefer local date.
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var dailyFileName = $"{fileNameWithoutExt}-{today}{extension}";

            // When no directory is configured, just return the file name in the current working directory.
            return string.IsNullOrEmpty(directory)
                ? dailyFileName
                : Path.Combine(directory, dailyFileName);
        }
    }
}
