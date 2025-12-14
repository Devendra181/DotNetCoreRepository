using System.ComponentModel.DataAnnotations.Schema;
namespace ECommerceApp.Models
{
    // Dependent entity in 1:M with Customer
    // Principal in 1:M with OrderItems
    public class Order : BaseAuditableEntity
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        // REQUIRED: Each Order should have a valid Status
        public int OrderStatusId { get; set; }
        public virtual OrderStatus OrderStatus { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Optional: Billing & Shipping addresses are Optional
        public int? ShippingAddressId { get; set; }
        public virtual Address? ShippingAddress { get; set; }
        public int? BillingAddressId { get; set; }
        public virtual Address? BillingAddress { get; set; }

        // REQUIRED: Who placed it
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        // 1:M → Each Order can have multiple items
        public virtual ICollection<OrderItem> OrderItems { get; set; } = null!;
    }
}
