using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CustomAuthFilterDemo.Filters
{
    // Custom authorization filter that checks user subscription level and expiry
    public class SubscriptionBasedAuthorizationFilter : IAuthorizationFilter
    {
        // Array of allowed subscription levels passed via constructor
        private readonly string[] _allowedSubscriptions;

        // Constructor accepting allowed subscription levels as params array
        public SubscriptionBasedAuthorizationFilter(params string[] allowedSubscriptions)
        {
            _allowedSubscriptions = allowedSubscriptions;
        }

        // This method is called by the ASP.NET Core pipeline to authorize a request
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Retrieve the current authenticated user principal from HttpContext
            var user = context.HttpContext.User;

            // Check if user is authenticated; if not, respond with 401 Unauthorized and JSON error message
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = CreateJsonResponse(
                    401,                              // HTTP status code
                    "Unauthorized",                   // Error title
                    "Authentication is required to access this resource."  // Detailed message
                );
                return; // Stop further processing since authorization failed
            }

            // Retrieve the "SubscriptionLevel" claim value from the user's claims
            var subscriptionLevel = user.FindFirst("SubscriptionLevel")?.Value;

            // Retrieve the "SubscriptionExpiresOn" claim value (subscription expiration date)
            var subExpires = user.FindFirst("SubscriptionExpiresOn")?.Value;

            // Check if the user's subscription level is NOT in the list of allowed subscriptions
            if (!_allowedSubscriptions.Contains(subscriptionLevel))
            {
                // Return 403 Forbidden with a custom JSON error message
                context.Result = CreateJsonResponse(
                    403,
                    "Forbidden",
                    "Your subscription level does not allow access to this resource."
                );
                return; // Stop further processing since authorization failed
            }

            // If the subscription expiry claim is present and can be parsed as a DateTime
            // and the subscription has expired (expiry date is in the past)
            if (subExpires != null && DateTime.TryParse(subExpires, out var exp) && exp < DateTime.UtcNow)
            {
                // Return 401 Unauthorized with a message about expired subscription
                context.Result = CreateJsonResponse(
                    401,
                    "Unauthorized",
                    "Your subscription has expired."
                );
                return; // Stop further processing
            }

            // If all checks pass, the request continues to the next stage in the pipeline
        }

        // Helper method to create a standardized JSON response with status code, error, and message
        private JsonResult CreateJsonResponse(int statusCode, string error, string message)
        {
            // Create an anonymous object representing the JSON payload
            var jsonPayload = new
            {
                Status = statusCode,  // HTTP status code (e.g., 401, 403)
                Error = error,        // Error type (e.g., "Unauthorized")
                Message = message     // Human-readable error message
            };

            // Return a JsonResult object with the payload and the appropriate HTTP status code
            return new JsonResult(jsonPayload)
            {
                StatusCode = statusCode
            };
        }
    }
}
