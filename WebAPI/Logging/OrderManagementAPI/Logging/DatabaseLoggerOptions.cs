namespace OrderManagementAPI.Logging
{
    // Strongly-typed configuration options for the Database Logger.
    // These values are usually bound from appsettings.json.
    public class DatabaseLoggerOptions
    {
        //The minimum log level that will be stored in the database.
        // Only logs with a level greater than or equal to this value
        // will be inserted into the LogEntries table.

        // Example:
        // - If set to Warning, then Warning, Error, and Critical logs are stored.
        // - Information and Debug logs are ignored for the database.
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Warning;
    }
}
