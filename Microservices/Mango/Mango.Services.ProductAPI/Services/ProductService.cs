using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Mango.Services.ProductAPI.Services.IService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Mango.Services.ProductAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public ProductService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductDto> GetProductById(int productId)
        {

            if(productId == 0)
            {
                return new ProductDto();
            }

            Product? product = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == productId);

            ProductDto productDto = _mapper.Map<ProductDto>(product);

            return productDto;
        }

        public async Task<List<ProductDto>> GetAllProducts()
        {
            List<Product> product = await _db.Products.ToListAsync();

            List<ProductDto> productDto = _mapper.Map<List<ProductDto>>(product);

            return productDto;
        }

        public async Task<ProductDto?> UpdateProduct(ProductDto newProduct)
        {
            var existingProduct = await GetProductById(newProduct.ProductId);
            if (existingProduct == null)
            {
                return null;
            }

            Product product = _mapper.Map<Product>(newProduct);

            _db.Products.Update(product);
            await _db.SaveChangesAsync();

            Product? updatedProduct = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == product.ProductId);

            return updatedProduct == null ? null: _mapper.Map<ProductDto>(updatedProduct);
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            bool isSuccess = false;
            Product? product = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == productId);

            if (product is not null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                isSuccess = true;
            }
            return isSuccess;
        }


        public async Task<ProductDto> InsertProduct(ProductDto createProduct)
        {

            if (createProduct == null)
            {
                return null;
            }
            var product = _mapper.Map<Product>(createProduct);

            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();

            return createProduct;
        }
    }
}
