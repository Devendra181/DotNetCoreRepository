using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using FluentValidation;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using eCommerce.ordersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
using eCommerce.OrdersService.BusinessLogicLayer.RabbitMQ;


namespace eCommerce.OrdersMicroservice.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        //TO DO: Add business logic layer services into the IoC container
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();

        services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);

        services.AddScoped<IOrdersService, eCommerce.OrdersMicroservice.BusinessLogicLayer.Services.OrdersService>();

        string connectionStringTemplate = configuration.GetConnectionString("RedisConnectionString")!;
        string connectionString = connectionStringTemplate.Replace("$REDIS_HOST", Environment.GetEnvironmentVariable("REDIS_HOST"))
            .Replace("$REDIS_PORT", Environment.GetEnvironmentVariable("REDIS_PORT"));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });

        services.AddTransient<IRabbitMQProductNameUpdateConsumer, RabbitMQProductNameUpdateConsumer>();

        services.AddHostedService<RabbitMQProductNameUpdateHostedService>();

        services.AddTransient<IRabbitMQProductDeletionConsumer, RabbitMQProductDeletionConsumer>();

        services.AddHostedService<RabbitMQProductDeletionHostedService>();

        return services;
    }
}
