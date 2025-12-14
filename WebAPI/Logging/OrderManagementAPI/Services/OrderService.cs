using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.DTOs;
using OrderManagementAPI.Entities;

namespace OrderManagementAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderManagementDbContext _dbContext;
        private readonly ILogger<OrderService> _logger;
        private readonly ICorrelationIdAccessor _correlationIdAccessor;

        public OrderService(
            OrderManagementDbContext dbContext,
            ILogger<OrderService> logger,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            _dbContext = dbContext;
            _logger = logger;
            _correlationIdAccessor = correlationIdAccessor;
        }

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO dto)
        {
            // Retrieve the current CorrelationId(if any) using the shared helper.
            var correlationId = _correlationIdAccessor.GetCorrelationId();

            _logger.LogInformation(
                $"[{correlationId}] Creating order for CustomerId {dto.CustomerId} with {dto.Items?.Count ?? 0} items.");

            // 1. Validate Customer (logs use string interpolation with CorrelationId)
            var customer = await _dbContext.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning(
                    $"[{correlationId}] Cannot create order: CustomerId {dto.CustomerId} not found.");
                throw new ArgumentException($"Customer with id {dto.CustomerId} not found.");
            }

            // Extra safety: ensure we actually have items
            if (dto.Items == null || dto.Items.Count == 0)
            {
                _logger.LogWarning(
                    $"[{correlationId}] Cannot create order: no items provided for CustomerId {dto.CustomerId}.");
                throw new ArgumentException("Order must contain at least one item.");
            }

            // 2. Get all product IDs from DTO and fetch from DB in one shot
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

            var products = await _dbContext.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();
                var missingIdsString = string.Join(", ", missingIds);

                _logger.LogWarning(
                    $"[{correlationId}] Cannot create order: some products not found or inactive. Missing IDs: {missingIdsString}.");

                throw new ArgumentException("One or more products are invalid or not active.");
            }

            // 3. Create Order and OrderItems
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.UtcNow
            };

            decimal total = 0;

            foreach (var itemDto in dto.Items)
            {
                var product = products.Single(p => p.Id == itemDto.ProductId);

                var unitPrice = product.Price;
                var lineTotal = unitPrice * itemDto.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                };

                order.Items.Add(orderItem);
                total += lineTotal;

                _logger.LogDebug(
                    $"[{correlationId}] Added item: ProductId={product.Id}, Quantity={itemDto.Quantity}, UnitPrice={unitPrice}, LineTotal={lineTotal}.");
            }

            order.TotalAmount = total;

            _logger.LogDebug(
                $"[{correlationId}] Total amount for CustomerId {dto.CustomerId} calculated as {order.TotalAmount}.");

            // 4. Save to DB with error logging
            try
            {
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    $"[{correlationId}] Order {order.Id} created successfully for CustomerId {dto.CustomerId}.");

                // Load navigation properties for mapping (Customer + Items + Product)
                await _dbContext.Entry(order).Reference(o => o.Customer).LoadAsync();
                await _dbContext.Entry(order).Collection(o => o.Items).LoadAsync();
                foreach (var item in order.Items)
                {
                    await _dbContext.Entry(item).Reference(i => i.Product).LoadAsync();
                }

                return MapToOrderDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"[{correlationId}] Error occurred while saving order for CustomerId {dto.CustomerId}.");
                throw; // let controller decide response
            }
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(int id)
        {
            var correlationId = _correlationIdAccessor.GetCorrelationId();

            _logger.LogInformation(
                $"[{correlationId}] Fetching order with OrderId {id}.");

            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsNoTracking()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning(
                    $"[{correlationId}] Order with OrderId {id} not found.");
                return null;
            }

            _logger.LogDebug(
                $"[{correlationId}] Order {order.Id} found for CustomerId {order.CustomerId}.");

            return MapToOrderDto(order);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersForCustomerAsync(int customerId)
        {
            var correlationId = _correlationIdAccessor.GetCorrelationId();

            _logger.LogInformation(
                $"[{correlationId}] Fetching orders for CustomerId {customerId}.");

            var orders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();

            _logger.LogInformation(
                $"[{correlationId}] Found {orders.Count} orders for CustomerId {customerId}.");

            return orders.Select(MapToOrderDto);
        }

        // Helper: Entity -> DTO mapping
        private static OrderDTO MapToOrderDto(Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName ?? string.Empty,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Items = order.Items.Select(i => new OrderItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? string.Empty,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList()
            };
        }
    }
}
