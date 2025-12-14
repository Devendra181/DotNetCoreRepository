using System.ComponentModel.DataAnnotations;
namespace ECommerceApp.Models
{
    // Dependent in the 1:1 relationship with Customer
    // PK = FK (same property) is the canonical required 1:1 pattern.
    public class Profile : BaseAuditableEntity
    {
        // Both Primary Key and Foreign Key to Customer
        [Key] // Keep this to make the PK explicit for readers
        public int CustomerId { get; set; }

        // Required 1:1 nav back to principal Customer
        public virtual Customer Customer { get; set; } = null!;

        // Extra profile info
        public string DisplayName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
    }
}
