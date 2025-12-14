using System.ComponentModel.DataAnnotations;
namespace OrderManagementAPI.Entities
{
    // Represents a single log entry stored in the database.
    public class LogEntry
    {
        public int Id { get; set; }

        // When the log was created (in UTC).
        public DateTime TimestampUtc { get; set; }

        // Log level as string: Information, Error, etc.
        [Required, MaxLength(20)]
        public string LogLevel { get; set; } = string.Empty;

        // Logger category - usually the class name (e.g., OrderService).
        [Required, MaxLength(256)]
        public string Category { get; set; } = string.Empty;

        // Main log message.
        [Required]
        public string Message { get; set; } = string.Empty;

        // Optional exception message if an error occurred.
        public string? ExceptionMessage { get; set; }

        // Optional exception stack trace for debugging.
        public string? ExceptionStackTrace { get; set; }

        // Store Correlation Id for each log
        public string? CorrelationId { get; set; }
    }
}

