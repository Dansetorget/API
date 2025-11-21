using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuizApi.DTOs;
using QuizApi.Models;

namespace QuizApi.Controllers
{
    [ApiController] // Configure the controller to act as an API and define its base URL: api/Auth
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AuthUser> _userManager; // Handles critical tasks like password hashing, salting, and validation
        private readonly SignInManager<AuthUser> _signInManager; // Session management and security features 
        private readonly IConfiguration _configuration; // Secure access to the application's settings, like the JWT secret key.
        private readonly ILogger<AuthController> _logger; // Writing log messages

        public AuthController(UserManager<AuthUser> userManager, SignInManager<AuthUser> signInManager, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        // Endpoint for user registration
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new AuthUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("[AuthController] Registration failed for {Username}", registerDto.Username);
                return BadRequest(result.Errors);
            }
            // Assign role user to newly registered users
            if (!await _userManager.IsInRoleAsync(user, "User"))
                await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("[AuthController] User registered: {Username}", registerDto.Username);
            return Ok(new { Message = "User registered successfully" });
        }

        // Endpoint for user login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username); // Find the user by username

            if (user == null)
            {
                _logger.LogWarning("[AuthController] Login failed (no such user) for {Username}", loginDto.Username);
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            var valid = await _userManager.CheckPasswordAsync(user, loginDto.Password); // Check if the provided password is correct
            if (!valid)
            {
                _logger.LogWarning("[AuthController] Login failed (invalid password) for {Username}", loginDto.Username);
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            var token = await GenerateJwtTokenAsync(user);
            _logger.LogInformation("[AuthController] Login success for {Username}", loginDto.Username);
            return Ok(new { Token = token }); // Return the JWT token to the client on successful login

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
        private async Task<string> GenerateJwtTokenAsync(AuthUser user)
        {
            var key = _configuration["Jwt:Key"]; // The secret key used for signing the token
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(key)) // Checking if the key is null or empty
            {
                _logger.LogError("[AuthAPIController] JWT key is missing from configuration.");
                throw new InvalidOperationException("JWT key is missing from configuration.");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)); // Reading the key from the configuration
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // Using HMAC SHA256 algorithm for signing the token

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // iat som seconds-since-epoch (UTC)
                new Claim(JwtRegisteredClaimNames.Iat,
                          DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                          ClaimValueTypes.Integer64)
            };

            // Include user roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(120), // Token expiration time set to 120 minutes
                signingCredentials: credentials); // Signing the token with the specified credentials

            _logger.LogInformation("[AuthAPIController] JWT token created for {@username}", user.UserName);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Endpoint for fetching user information
        // Fetch info only for the logged in user, but can be extended to fetch any user's info by ID or username
        [Authorize]
        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user ID from the JWT claims
            if (userId == null)
            {
                return NotFound("No logged-in user found.");
            }

            var user = await _userManager.FindByIdAsync(userId); // Find the user by ID
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var roles = await _userManager.GetRolesAsync(user); // Get the user's roles

            var userInfo = user.Adapt<UserInfoDTO>();
            userInfo.Roles = roles.ToList();

            return Ok(userInfo);
        }


        // Endpoint for changing password
        // Patch api/Auth/changepassword
        [Authorize]
        [HttpPatch("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("No logged-in user found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword
            );

            if (!result.Succeeded)
            {
                _logger.LogWarning("[AuthController] Password change failed for {Username}", user.UserName);

                return BadRequest(new
                {
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation("[AuthController] Password changed successfully for {Username}", user.UserName);

            return Ok(new { Message = "Password changed successfully" });
        }

        // Endpoint for changing username
        // Patch api/Auth/changeusername
        [Authorize]
        [HttpPatch("changeusername")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDTO changeUsernameDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("No logged-in user found.");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check that new username is different from current username
            var newUsername = changeUsernameDto.NewUsername.Trim();
            if (string.Equals(newUsername, user.UserName, StringComparison.OrdinalIgnoreCase))
                return BadRequest("New username must be different from current username.");

            // Check if the new username is already taken
            var existing = await _userManager.FindByNameAsync(newUsername);
            if (existing != null)
                return Conflict("Username already in use.");

            // Update the username
            try
            {
                await _userManager.SetUserNameAsync(user, newUsername);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error changing username for {UserId}", userId);
                return StatusCode(500, "An error occurred while changing the username.");
            }

            // Update security stamp so tokens can be invalidated if desired
            await _userManager.UpdateSecurityStampAsync(user);

            _logger.LogInformation("Username changed for user {UserId} => {NewUsername}", userId, newUsername);
            var newToken = await GenerateJwtTokenAsync(user);
            return Ok(new { Message = "Username changed successfully", Token = newToken });
        }


        // Delete user account
        [Authorize]
        [HttpDelete("deleteaccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("No logged-in user found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("[AuthController] Account deletion failed for {Username}", user.UserName);
                return BadRequest(new
                {
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation("[AuthController] Account deleted successfully for {Username}", user.UserName);
            return Ok(new { Message = "Account deleted successfully" });
        }
    }
}