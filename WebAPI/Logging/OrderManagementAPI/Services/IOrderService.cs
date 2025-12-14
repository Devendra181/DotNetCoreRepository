using OrderManagementAPI.DTOs;
namespace OrderManagementAPI.Services
{
    public interface IOrderService
    {
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO dto);
        Task<OrderDTO?> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDTO>> GetOrdersForCustomerAsync(int customerId);
    }
}
