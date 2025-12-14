namespace OrderManagementAPI.Services
{
    public class CorrelationIdAccessor : ICorrelationIdAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Key used by the CorrelationId middleware to store the value in HttpContext.Items.
        private const string CorrelationIdItemKey = "X-Correlation-ID";

        public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCorrelationId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            // Try to read the CorrelationId set by middleware.
            if (httpContext.Items.TryGetValue(CorrelationIdItemKey, out var value) &&
                value is string cidFromItems)
            {
                return cidFromItems;
            }

            // Fallback: use the ASP.NET Core generated TraceIdentifier.
            return httpContext.TraceIdentifier;
        }
    }
}
