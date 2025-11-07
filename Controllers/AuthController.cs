using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuizApi.DTOs;
using QuizApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuizApi.Controllers
{
    [ApiController] // Configure the controller to act as an API and define its base URL: api/Auth
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AuthUser> _userManager; // Handles critical tasks like password hashing, salting, and validation
        private readonly SignInManager<AuthUser> _signInManager; // Session management and security features 
        private readonly IConfiguration _configuration; // Secure access to your application's settings, like the JWT secret key.
        private readonly ILogger<AuthController> _logger; // Writing log messages

        public AuthController(UserManager<AuthUser> userManager, SignInManager<AuthUser> signInManager, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        // Endpoint for user registration
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new AuthUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                // FirstName = registerDto.FirstName,
                // LastName = registerDto.LastName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("[AuthAPIController] user registered for {@username}", registerDto.Username);
                return Ok(new { Message = "User registered successfully" });
            }

            _logger.LogWarning("[AuthAPIController] user registration failed for {@username}", registerDto.Username);
            return BadRequest(result.Errors);
        }

        // Endpoint for user login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                _logger.LogInformation("[AuthAPIController] user authorised for {@username}", loginDto.Username);
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            _logger.LogWarning("[AuthAPIController] user not authorised for {@username}", loginDto.Username);
            return Unauthorized();
        }

        [Authorize]
        // Endpoint for user logout (actual "logout" is performed by the client, which discards the token.)
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // This endpoint can be used for things like token invalidation if implemented.
            await _signInManager.SignOutAsync();
            _logger.LogInformation("[AuthAPIController] user logged out");
            return Ok(new { Message = "Logout successful" });
        }

        // A private helper method responsible for creating the JWT.
        private string GenerateJwtToken(AuthUser user)
        {
            var jwtKey = _configuration["Jwt:Key"]; // The secret key used for signing the token
            if (string.IsNullOrEmpty(jwtKey)) // Checking if the key is null or empty
            {
                _logger.LogError("[AuthAPIController] JWT key is missing from configuration.");
                throw new InvalidOperationException("JWT key is missing from configuration.");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)); // Reading the key from the configuration
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // Using HMAC SHA256 algorithm for signing the token

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique identifier for the token
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // Issued at timestamp
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120), // Token expiration time set to 120 minutes
                signingCredentials: credentials); // Signing the token with the specified credentials

            _logger.LogInformation("[AuthAPIController] JWT token created for {@username}", user.UserName);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}