using System.ComponentModel.DataAnnotations;
namespace ECommerceApp.DTOs
{
    // Represents the full data required to place a new order
    public class OrderRequestDTO
    {
        // The customer placing the order
        [Required(ErrorMessage = "CustomerId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be a positive integer.")]
        public int CustomerId { get; set; }

        // Chosen shipping address
        [Required(ErrorMessage = "ShippingAddressId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShippingAddressId must be a positive integer.")]
        public int ShippingAddressId { get; set; }

        // Chosen billing address
        [Required(ErrorMessage = "BillingAddressId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BillingAddressId must be a positive integer.")]
        public int BillingAddressId { get; set; }

        // List of products in the order
        [Required(ErrorMessage = "Order must contain at least one item.")]
        [MinLength(1, ErrorMessage = "At least one order item must be provided.")]
        public List<OrderItemRequestDTO> Items { get; set; } = new();
    }
}
