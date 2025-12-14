using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerceApp.Models
{
    // Principal for Profile, Addresses, and Orders
    public class Customer : BaseAuditableEntity
    {
        // Primary Key (use explicit name to be clear)
        public int CustomerId { get; set; }

        // Basic Customer Info (Required by default through non-nullable refs)
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;

        // 1:1 → A customer may have one Profile
        public virtual Profile? Profile { get; set; }

        // 1:M → A customer can have many addresses and orders
        public virtual ICollection<Address>? Addresses { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }
}
