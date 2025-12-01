
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }
    public async Task<UserDTO?> GetUserByUserIDAsync(Guid userId)
    {
        try
        {

            //Key: user:123
            //Value: {"UserName: "...", .... }

            string cacheKey = $"user:{userId}";
            string? cachedUser = await _distributedCache.GetStringAsync(cacheKey);

            if(cachedUser != null)
            {
                UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO>(cachedUser);

                return userFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                if(response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    UserDTO? userDTOBulkhead = await response.Content.ReadFromJsonAsync<UserDTO>();

                    if(userDTOBulkhead == null)
                    {
                        throw new NotImplementedException($"Bulkhead policy was not implemented");
                    }

                    return userDTOBulkhead;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException($"Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                //throw new HttpRequestException($"Http request failed with status code {response.StatusCode} ");

                return new UserDTO(
                    PersonName: "Temporarily Unavailable",
                    Email: "Temporarily Unavailable",
                    Gender: "Temporarily Unavailble",
                    UserID: Guid.Empty
                    );
                }
            }

            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

            if (user == null)
            {
                throw new HttpRequestException($"Invalid User ID", null);
            }

            string userJson = JsonSerializer.Serialize(user);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                 .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddSeconds(60)) // Cache for 30 seconds
                 .SetSlidingExpiration(TimeSpan.FromSeconds(30)); // Sliding expiration of 10 seconds
           

            await _distributedCache.SetStringAsync(cacheKey, userJson, options);

            return user;
        }
        catch(BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Request failed because of circuit breaker is in Open state. Returning dummy");
            return new UserDTO(
                    PersonName: "Temporarily Unavailable (circuit breaker)",
                    Email: "Temporarily Unavailable (circuit breaker)",
                    Gender: "Temporarily Unavailble (circuit breaker)",
                    UserID: Guid.Empty
                    );
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timeout occured while fetching user data. Returning dummy data");
            return new UserDTO(
                    PersonName: "Temporarily Unavailable (Timeout)",
                    Email: "Temporarily Unavailable (Timeout)",
                    Gender: "Temporarily Unavailble (Timeout)",
                    UserID: Guid.Empty
                    );
        }
    }
}
