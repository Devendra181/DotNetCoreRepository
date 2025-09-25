using Mango.Services.ProductAPI.Models.Dto;

namespace Mango.Services.ProductAPI.Services.IService
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProducts();
        Task<ProductDto> GetProductById(int productId);
        Task<ProductDto?> UpdateProduct(ProductDto product);
        Task<bool> DeleteProduct(int productId);
        Task<ProductDto> InsertProduct(ProductDto product);
    }
}
