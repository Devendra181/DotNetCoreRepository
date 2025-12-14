using System.ComponentModel.DataAnnotations.Schema;
namespace ECommerceApp.Models
{
    // Dependent in 1:M with Category, and principal for OrderItems
    public class Product : BaseAuditableEntity
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string SKU { get; set; } = null!;
        public string? Description { get; set; }

        // Required FK to Category (1 Product belongs to 1 Category)
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        // Many-to-Many → OrderItem acts as the bridge entity
        public virtual ICollection<OrderItem>? OrderItems { get; set; }
    }
}
