using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CustomAuthFilterDemo.Filters
{
    // Custom authorization attribute to restrict API access based on business hours
    // Inherits from AuthorizeAttribute and implements IAuthorizationFilter for synchronous authorization logic
    public class BusinessHoursAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly int _startHour; // Start of allowed access window (24-hour format)
        private readonly int _endHour;   // End of allowed access window (24-hour format)

        // Constructor to initialize business hours; defaults to 9 AM to 6 PM
        public BusinessHoursAuthorizeAttribute(int startHour = 9, int endHour = 18)
        {
            _startHour = startHour;
            _endHour = endHour;
        }

        // This method is called during request processing to perform authorization check
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Get the current user principal from the HTTP context
            var user = context.HttpContext.User;

            // Check if the user is NOT authenticated (null or not authenticated)
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                // Return 401 Unauthorized with a standardized JSON error response
                context.Result = CreateJsonResponse(
                    401,                                   // HTTP status code
                    "Unauthorized",                        // Error type string
                    "Authentication is required to access this resource."  // Human-readable message
                );
                return; // Stop further pipeline execution
            }

            // Get the current local time of day
            var now = DateTime.Now.TimeOfDay;

            // Check if current hour is outside the allowed business hours range
            if (now.Hours < _startHour || now.Hours >= _endHour)
            {
                // Return 403 Forbidden with JSON error indicating access restriction by time
                context.Result = CreateJsonResponse(
                    403,
                    "Forbidden",
                    $"API accessible only between {_startHour}:00 and {_endHour}:00 local time."
                );
                return; // Stop further pipeline execution
            }

            // If user is authenticated and current time is within business hours, request proceeds normally
        }

        // Helper method to generate consistent JSON error responses with given status code, error, and message
        private JsonResult CreateJsonResponse(int statusCode, string error, string message)
        {
            // Create anonymous object to hold the response payload
            var jsonPayload = new
            {
                Status = statusCode, // HTTP status code e.g. 401 or 403
                Error = error,       // Error string like "Unauthorized" or "Forbidden"
                Message = message    // Human-readable explanation for client
            };

            // Return a JsonResult with the payload and specified HTTP status code
            return new JsonResult(jsonPayload)
            {
                StatusCode = statusCode
            };
        }
    }
}
