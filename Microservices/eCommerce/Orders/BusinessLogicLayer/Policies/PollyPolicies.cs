
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class PollyPolicies : IPollyPolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;
    public PollyPolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy =
        Policy.HandleResult<HttpResponseMessage>(respose => !respose.IsSuccessStatusCode)
    .WaitAndRetryAsync(
        retryCount: retryCount, //Number of retries
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(1, retryAttempt)), //Delay between retries
        onRetry: (outcome, timeSpan, retryAttemp, context) =>
        {
            _logger.LogInformation($"Retry {retryAttemp} after {timeSpan.TotalSeconds} Seconds");
        });

        return retryPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
            AsyncCircuitBreakerPolicy<HttpResponseMessage> retryPolicy =
           Policy.HandleResult<HttpResponseMessage>(respose => !respose.IsSuccessStatusCode)
       .CircuitBreakerAsync(
           handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking, //Number of retries
           durationOfBreak: durationOfBreak, //Delay between retries
           onBreak: (outcome, timeSpan) =>
           {
               _logger.LogInformation($"Circuit breaker opened for {timeSpan.TotalMinutes} minutes due to consecutive 3 failures. The subequest request will be blocked");
           },
           onReset: () =>
           {
               _logger.LogInformation($"Circuit breaker reset. The subequest request will be allowed again");
           });

            return retryPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);

        return policy;
    }

    
}
