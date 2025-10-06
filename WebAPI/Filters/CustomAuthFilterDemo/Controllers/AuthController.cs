using CustomAuthFilterDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomAuthFilterDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration; // To access configuration settings like JWT secret key

        // Constructor to inject IConfiguration via dependency injection
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST api/auth/login endpoint - publicly accessible (no authentication required)
        [HttpPost("login")]
        [AllowAnonymous] // Allows this action to be called without authentication
        public IActionResult Login([FromBody] LoginDTO login) // Accepts login data from request body
        {
            // Try to find a user in your in-memory store matching email (case-insensitive) and password
            var user = UserStore.Users.FirstOrDefault(u =>
                    u.Email.Equals(login.Email, StringComparison.OrdinalIgnoreCase)
                    && u.Password == login.Password);

            if (user == null)
            {
                // No user found with given credentials - respond with HTTP 401 Unauthorized and message
                return Unauthorized("Invalid username or password");
            }

            // Create claims to embed in the JWT token for identity and authorization info
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email), // Standard claim for username/email
                new Claim("SubscriptionLevel", user.SubscriptionLevel ?? "Free"), // Custom claim for subscription type (default Free)
                new Claim("Department", user.Department ?? "None") // Custom claim for department (default None)
            };

            // If user has a subscription expiration date, add it as a claim in ISO 8601 string format
            if (user.SubscriptionExpiresOn != null)
                claims.Add(new Claim("SubscriptionExpiresOn", user.SubscriptionExpiresOn.Value.ToString()));

            // Read the JWT secret key from configuration; fallback to hardcoded key if missing (for development)
            var secretKey = _configuration.GetValue<string>("JwtSettings:SecretKey")
                            ?? "d3011f8b98bbc1aa1c4ff1a7d4864fc72d9ee150bd682cf4e612d6321f57821d";

            // Convert the secret key string into a byte array and create a SymmetricSecurityKey instance
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Create signing credentials specifying the key and the HMAC SHA256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token object with claims, expiration (30 mins), and signing credentials
            var token = new JwtSecurityToken(
                issuer: null, // No issuer specified (optional)
                audience: null, // No audience specified (optional)
                claims: claims, // The claims added earlier
                expires: DateTime.UtcNow.AddMinutes(30), // Token valid for 30 minutes from now
                signingCredentials: creds); // Use the signing credentials for security

            // Serialize the token object into a JWT compact string format
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Return the token string inside an anonymous object as JSON with HTTP 200 OK status
            return Ok(new { Token = tokenString });
        }
    }
}
