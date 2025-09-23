
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersService.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateConsumer : IDisposable, IRabbitMQProductNameUpdateConsumer
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;
    private readonly IDistributedCache _distributedCache;

    public RabbitMQProductNameUpdateConsumer(IConfiguration configuration, ILogger<RabbitMQProductNameUpdateConsumer> logger, IDistributedCache distributedCache)
    {
        _configuration = configuration;

        Console.WriteLine($"RabbitMQ_HostName: {_configuration["RabbitMQ_HostName"]}");
        Console.WriteLine($"RabbitMQ_UserName: {_configuration["RabbitMQ_UserName"]}");
        Console.WriteLine($"RabbitMQ_Password: {_configuration["RabbitMQ_Password"]}");
        Console.WriteLine($"RabbitMQ_Port: {_configuration["RabbitMQ_Port"]}");

        string hostName = _configuration["RabbitMQ_HostName"]!;
        string userName = _configuration["RabbitMQ_UserName"]!;
        string password = _configuration["RabbitMQ_Password"]!;
        //string port = _configuration["RabbitMQ_Port"]!;
        string port = System.Environment.GetEnvironmentVariable("RabbitMQ_Port");
        _logger = logger;

        ConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };
        _connection = connectionFactory.CreateConnection();

        _channel = _connection.CreateModel();
        _distributedCache = distributedCache;
    }


    public void Consume()
    {

        //string routingKey = "product.update.name";
        var headers = new Dictionary<string, object>()
          {
            { "x-match", "all" },
            { "event", "product.update" },
            { "RowCount", 1 }
          };

        string queueName = "orders.product.update.name.queue";

        //Create exchange
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

        //Create message queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null); //x-message-ttl | x-max-length | x-expired 

        //Bind the message to exchange
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);


        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            if (message != null)
            {
                ProductDTO? productDTO = JsonSerializer.Deserialize<ProductDTO>(message);

                if(productDTO != null)
                {
                    //TO DO: Update product cache
                   await HandleProductUpdation(productDTO);
                }
            }
        };

        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
    }

    private async Task HandleProductUpdation(ProductDTO productDTO) 
    {
        _logger.LogInformation($"Product name updated: {productDTO.ProductID}, New name: {productDTO.ProductName}");

        string cacheKey = $"product:{productDTO.ProductID}";

        string productJson = JsonSerializer.Serialize(productDTO);

        DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(3))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        await _distributedCache.SetStringAsync(cacheKey, productJson, options);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}