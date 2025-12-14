using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Enums;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ECommerceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // =================================================================
        // POST: api/order/place
        // PURPOSE: Places a new order with validation and business rules
        // Demonstrates: Validations, relationships, transactions, soft audit fields
        // =================================================================
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDTO request)
        {
            try
            {
                // STEP 1: Validate Customer existence
                var customer = await _context.Customers
                    .Include(c => c.Addresses)
                    .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId && c.IsActive);
                if (customer == null)
                    return BadRequest($"Customer {request.CustomerId} not found or inactive.");

                // STEP 2: Validate Shipping & Billing Addresses (must belong to the same customer)
                var shipping = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddressId == request.ShippingAddressId && a.CustomerId == request.CustomerId);
                var billing = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddressId == request.BillingAddressId && a.CustomerId == request.CustomerId);

                if (shipping == null || billing == null)
                    return BadRequest("Invalid Shipping or Billing Address for this customer.");

                // STEP 3: Validate Products & Stock availability
                var productIds = request.Items.Select(i => i.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.ProductId) && p.IsActive)
                    .ToListAsync();

                // Check product existence
                if (products.Count != request.Items.Count)
                    return BadRequest("Some products are invalid or inactive.");

                // Check stock for each product
                foreach (var item in request.Items)
                {
                    var product = products.First(p => p.ProductId == item.ProductId);
                    if (product.Stock < item.Quantity)
                        return BadRequest($"Insufficient stock for '{product.Name}'. Available: {product.Stock}, Requested: {item.Quantity}");
                }

                // STEP 4: Create new Order object
                var order = new Order
                {
                    CustomerId = request.CustomerId,
                    ShippingAddressId = request.ShippingAddressId,
                    BillingAddressId = request.BillingAddressId,
                    OrderStatusId = (int)OrderStatusEnum.Pending,
                    OrderDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsActive = true
                };

                // STEP 5: Map Order Items and Deduct Stock
                order.OrderItems = new List<OrderItem>();

                foreach (var item in request.Items)
                {
                    var product = products.First(p => p.ProductId == item.ProductId);

                    // Reduce stock from product
                    product.Stock -= item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    product.UpdatedBy = "System";

                    // Create order item record
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price // use price from DB, not client
                    });
                }

                // STEP 6: Compute total amount
                order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);

                // STEP 7: Save changes (transactionally)
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // STEP 8: Return success response
                return Ok(new
                {
                    Message = "Order placed successfully.",
                    OrderId = order.OrderId
                });
            }
            catch (Exception ex)
            {
                // Global exception handler
                return StatusCode(500, new
                {
                    Message = "Unexpected error while placing the order.",
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}

