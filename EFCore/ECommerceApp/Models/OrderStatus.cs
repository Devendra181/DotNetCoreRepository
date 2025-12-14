namespace ECommerceApp.Models
{
    // Represents a master table for all possible order statuses.
    // The enum values are seeded here for database persistence.
    public class OrderStatus : BaseAuditableEntity
    {
        public int OrderStatusId { get; set; }     // Primary Key
        public string Name { get; set; } = null!;  // e.g., "Pending", "Shipped", etc.
        public string? Description { get; set; }

        // Navigation Property (1:M) → One status can be assigned to many orders
        public virtual ICollection<Order>? Orders { get; set; }
    }
}
