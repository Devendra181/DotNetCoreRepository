namespace ECommerceApp.Models
{
    // Dependent in 1:M with Customer
    public class Address : BaseAuditableEntity
    {
        public int AddressId { get; set; }
        public string Line1 { get; set; } = null!;
        public string? Line2 { get; set; }
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;

        // REQUIRED Relationship: Every Address must belong to a Customer
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;
    }
}
