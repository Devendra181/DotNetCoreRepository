using eCommerce.BusinessLogicLayer.Mappers;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Validators;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        // Add Business Logic Layer services into the IoC container
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ProductAddRequestToProductMappingProfile).Assembly));

        services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();

        services.AddScoped<IProductsService, eCommerce.BusinessLogicLayer.Services.ProductsService>();

        services.AddTransient<IRabbitMQPublisher, RabbitMQPublisher>();

        return services;
    }
}
        
