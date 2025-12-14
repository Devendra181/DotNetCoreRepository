using System.ComponentModel.DataAnnotations;
namespace ECommerceApp.DTOs
{
    // Represents an individual product item in a new order request
    public class OrderItemRequestDTO
    {
        // The product being ordered
        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer.")]
        public int ProductId { get; set; }

        // Quantity of that product
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
        public int Quantity { get; set; }
    }
}
