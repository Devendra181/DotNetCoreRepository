namespace OrderManagementAPI.Logging
{
    // Strongly-typed configuration options for the Text File Logger.
    // These values are usually bound from appsettings.json.
    public class TextFileLoggerOptions
    {
        // Base path of the log file where entries will be written.
        // Example: "Logs/order-api-log.txt"
        // If the directory does not exist, the logger will attempt to create it.
        public string FilePath { get; set; } = "Logs/order-api-log.txt";

        // The minimum log level that will be written to the text file.
        // Only logs with a level greater than or equal to this value
        // will be appended to the file.
        // Example:
        // - If set to Information, then Information, Warning, Error, and Critical logs are written.
        // - Debug and Trace logs are ignored for the file.
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

        // Controls whether the logger should create a separate log file for each day.
        // When true:
        //   - The logger will append the current date to the base file name.
        //   - Example:
        //       FilePath              = "Logs/order-api-log.txt"
        //       Actual file (today)   = "Logs/order-api-log-2025-12-10.txt"
        // When false:
        //   - All log entries are written to the single file specified in FilePath,
        //     without any date suffix.
        public bool UseDailyRollingFiles { get; set; } = true;
    }
}
