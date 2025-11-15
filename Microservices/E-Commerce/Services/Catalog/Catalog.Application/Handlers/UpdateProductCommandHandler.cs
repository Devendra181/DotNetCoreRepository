using Catalog.Application.Commands;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;

        public UpdateProductCommandHandler(IProductRepository productRepository)
        {
            this._productRepository = productRepository;
        }

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            //Pleasee use auto mapper in real time projects, instead of below classic way of doing object mapping

            var productEntity = await _productRepository.UpdateProduct(new Product
            {
                Id = request.Id,
                Description = request.Description,
                ImageFile = request.ImageFile,
                Name = request.Name,
                price = request.Price,
                Summary = request.Summary,
                Brands = request.Brands,
                Type = request.Types
            });

            return true;
        }
        
    }
}
