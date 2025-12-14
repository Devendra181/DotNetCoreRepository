namespace ECommerceApp.Models
{
    // Principal in 1:M with Product
    public class Category : BaseAuditableEntity
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        // One Category → Many Products
        public virtual ICollection<Product>? Products { get; set; }
    }
}
