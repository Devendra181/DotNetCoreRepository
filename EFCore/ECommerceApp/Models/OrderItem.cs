using System.ComponentModel.DataAnnotations.Schema;
namespace ECommerceApp.Models
{
    // Dependent in 1:M with Order
    // Dependent in 1:M with Product
    // This is the Joining Entity enabling a Many-to-Many between Orders and Products.
    public class OrderItem : BaseAuditableEntity
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // snapshot of price at purchase time

        // Foreign Keys
        // REQUIRED: each OrderItem belongs to an Order
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        // REQUIRED: each OrderItem refers to a Product
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
    }
}
