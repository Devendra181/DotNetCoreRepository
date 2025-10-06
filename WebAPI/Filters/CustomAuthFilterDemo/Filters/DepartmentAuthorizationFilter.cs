using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CustomAuthFilterDemo.Filters
{
    // Custom asynchronous authorization filter that restricts access based on user's department claim
    public class DepartmentAuthorizationFilter : IAsyncAuthorizationFilter
    {
        // The department allowed to access the resource, passed via constructor
        private readonly string _allowedDepartment;

        // Constructor to initialize the allowed department value
        public DepartmentAuthorizationFilter(string allowedDepartment)
        {
            _allowedDepartment = allowedDepartment;
        }

        // This async method is called during request processing to authorize access
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Retrieve the current authenticated user principal from the HTTP context
            var user = context.HttpContext.User;

            // Check if the user is NOT authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                // User is not authenticated - respond with 401 Unauthorized JSON error
                context.Result = CreateJsonResponse(
                    401,                             // HTTP status code
                    "Unauthorized",                  // Error title
                    "Authentication is required to access this resource."  // Message
                );
                return; // Stop further pipeline execution
            }

            // Retrieve the "Department" claim value from the user's claims
            var department = user.FindFirst("Department")?.Value;

            // Check if department claim is missing or does NOT match the allowed department (case-insensitive)
            if (department == null || !string.Equals(department, _allowedDepartment, StringComparison.OrdinalIgnoreCase))
            {
                // Return 403 Forbidden with a JSON error indicating department restriction
                context.Result = CreateJsonResponse(
                    403,
                    "Forbidden",
                    $"Access restricted to {_allowedDepartment} department only."
                );
                return; // Stop further pipeline execution
            }

            // Since this method is async, complete the task to satisfy interface contract
            await Task.CompletedTask;
        }

        // Helper method to create a JSON response with status, error, and message fields
        private JsonResult CreateJsonResponse(int statusCode, string error, string message)
        {
            // Create anonymous object to represent JSON payload
            var jsonPayload = new
            {
                Status = statusCode,  // HTTP status code (401 or 403)
                Error = error,        // Error type string
                Message = message     // Human-readable message
            };

            // Return the JSON response with appropriate HTTP status code
            return new JsonResult(jsonPayload)
            {
                StatusCode = statusCode
            };
        }
    }
}
