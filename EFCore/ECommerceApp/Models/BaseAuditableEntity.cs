namespace ECommerceApp.Models
{
    // Base class that provides soft-delete and audit tracking.
    // Every domain entity inherits this for consistency.
    public abstract class BaseAuditableEntity
    {
        // Indicates whether the record is active (used for soft delete)
        public bool IsActive { get; set; } = true;

        // Audit Columns
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // Record creation timestamp
        public DateTime? UpdatedAt { get; set; }                     // Last update timestamp
        public string? CreatedBy { get; set; }                       // Optional: can be set by middleware later
        public string? UpdatedBy { get; set; }                       // Optional: for tracking modifications
    }
}
