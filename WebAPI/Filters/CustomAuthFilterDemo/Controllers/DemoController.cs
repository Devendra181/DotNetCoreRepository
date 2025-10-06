using Microsoft.AspNetCore.Mvc;
using CustomAuthFilterDemo.Filters;

namespace CustomAuthFilterDemo.Controllers
{
    [ApiController]
    [Route("api/demo")]
    public class DemoController : ControllerBase
    {
        // Endpoint protected by subscription-based filter requiring "Premium" or "Pro"
        [HttpGet("premium-analytics")]     // Maps HTTP GET to /api/demo/premium-analytics
        [TypeFilter(typeof(SubscriptionBasedAuthorizationFilter), Arguments = new object[] { new[] { "Premium", "Pro" } })]
        // Uses TypeFilter to apply SubscriptionBasedAuthorizationFilter with allowed subscriptions "Premium" and "Pro"
        public IActionResult GetPremiumAnalytics()
        {
            // Return HTTP 200 OK with JSON message confirming access to premium analytics
            return Ok(new { message = "Welcome to Premium Analytics." });
        }

        // Endpoint restricted to users from the "HR" department only
        [HttpGet("salary-review")]         // Maps HTTP GET to /api/demo/salary-review
        [TypeFilter(typeof(DepartmentAuthorizationFilter), Arguments = new object[] { "HR" })]
        // Applies DepartmentAuthorizationFilter with allowed department "HR" via TypeFilter
        public IActionResult GetSalaryReview()
        {
            // Return HTTP 200 OK with JSON message containing salary review data
            return Ok(new { message = "HR department salary review data." });
        }

        // Endpoint accessible only during specified business hours (9 AM - 6 PM UTC)
        [HttpGet("support-ticket")]        // Maps HTTP GET to /api/demo/support-ticket
        [BusinessHoursAuthorize(9, 18)]    // Applies custom time-based authorization attribute restricting access to 9 AM to 6 PM
        public IActionResult GetSupportTicket()
        {
            // Return HTTP 200 OK with JSON message confirming access during business hours
            return Ok(new { message = "Support ticket API (business hours only) accessed." });
        }
    }
}
