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
using Microsoft.AspNetCore.Http;

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
                _logger.LogWarning("Login failed (no such user) for {Username}", loginDto.Username);
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            var valid = await _userManager.CheckPasswordAsync(user, loginDto.Password); // Check if the provided password is correct
            if (!valid)
            {
                _logger.LogWarning("Login failed (invalid password) for {Username}", loginDto.Username);
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            var token = await GenerateJwtTokenAsync(user);
            _logger.LogInformation("Login success for {Username}", loginDto.Username);
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
            // Get the current user
            var (user, userId, error) = await GetCurrentUser();
            if (error != null) return error;
            var currentUser = user!; // Already checked for null above

            var roles = await _userManager.GetRolesAsync(currentUser); // Get the user's roles

            var userInfo = currentUser.Adapt<UserInfoDTO>();
            userInfo.Roles = roles.ToList();
            userInfo.AvatarId = currentUser.AvatarId ?? 0;
            userInfo.AvatarUrl = currentUser.AvatarId.HasValue ? $"/avatars/{currentUser.AvatarId}.svg" : null; // Frontend uses this url to display the avatar
            return Ok(userInfo);
        }


        // Endpoint for changing username
        // Patch api/Auth/changeusername
        [Authorize]
        [HttpPatch("changeusername")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDTO changeUsernameDto)
        {
            // Get the current user
            var (user, userId, error) = await GetCurrentUser();
            if (error != null) return error;
            var currentUser = user!; // Already checked for null above

            // Check that new username is different from current username
            var newUsername = changeUsernameDto.NewUsername.Trim();
            if (string.Equals(newUsername, currentUser.UserName, StringComparison.OrdinalIgnoreCase))
                return BadRequest("New username must be different from current username.");

            // Check if the new username is already taken
            var existing = await _userManager.FindByNameAsync(newUsername);
            if (existing != null)
                return Conflict("Username already in use.");

            // Update the username
            var setNewUsername = await _userManager.SetUserNameAsync(currentUser, newUsername);
            if (!setNewUsername.Succeeded)
            {
                _logger.LogWarning("Could not change username for {UserId}: {@Errors}", userId, setNewUsername.Errors);
                return BadRequest(setNewUsername.Errors);
            }

            // Update security stamp so tokens can be invalidated if desired
            await _userManager.UpdateSecurityStampAsync(currentUser);

            _logger.LogInformation("Username changed for user {UserId} => {NewUsername}", userId, newUsername);
            var newToken = await GenerateJwtTokenAsync(currentUser);
            return Ok(new { Message = "Username changed successfully", Token = newToken });
        }

        // Endpoint for changing email
        // Patch api/Auth/changeemail
        [Authorize]
        [HttpPatch("changeemail")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDTO changeEmailDto)
        {
            // Get the current user
            var (user, userId, error) = await GetCurrentUser();
            if (error != null) return error;
            var currentUser = user!; // Already checked for null above

            var newEmail = changeEmailDto.NewEmail.Trim();

            // Check if the new email is already taken
            var existing = await _userManager.FindByEmailAsync(newEmail);
            if (existing != null)
                return Conflict("Email already in use.");

            var setNewEmail = await _userManager.SetEmailAsync(currentUser, newEmail);
            if (!setNewEmail.Succeeded)
            {
                _logger.LogWarning("Could not change email for {UserId}: {@Errors}", userId, setNewEmail.Errors);
                return BadRequest(setNewEmail.Errors);
            }
            // Update security stamp so tokens can be invalidated if desired
            await _userManager.UpdateSecurityStampAsync(currentUser);

            var newToken = await GenerateJwtTokenAsync(currentUser);

            _logger.LogInformation("Email changed for user {UserId} => {NewEmail}", userId, newEmail);
            return Ok(new { Message = "Email changed successfully.", Token = newToken });

        }

        // Endpoint for changing password
        // Patch api/Auth/changepassword
        [Authorize]
        [HttpPatch("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDto)
        {
            // Get the current user
            var (user, userId, error) = await GetCurrentUser();
            if (error != null) return error;
            var currentUser = user!; // Already checked for null above

            var result = await _userManager.ChangePasswordAsync(
                currentUser,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword
            );

            if (!result.Succeeded)
            {
                _logger.LogWarning("[AuthController] Password change failed for {Username}", currentUser.UserName);

                return BadRequest(new
                {
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation("[AuthController] Password changed successfully for {Username}", currentUser.UserName);

            return Ok(new { Message = "Password changed successfully" });
        }

        // Delete user account
        [Authorize]
        [HttpDelete("deleteaccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            // Get the current user
            var (user, userId, error) = await GetCurrentUser();
            if (error != null) return error;
            var currentUser = user!; // Already checked for null above

            // Delete the user account
            var result = await _userManager.DeleteAsync(currentUser);
            if (!result.Succeeded)
            {
                _logger.LogWarning("[AuthController] Account deletion failed for {Username}", currentUser.UserName);
                return BadRequest(new
                {
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation("[AuthController] Account deleted successfully for {Username}", currentUser.UserName);
            return Ok(new { Message = "Account deleted successfully" });
        }

        [Authorize]
        [HttpPatch("changeavatar")]
        public async Task<IActionResult> SetAvatar([FromBody] int avatarId)
        {
            // Validate avatarId is in valid range (1-5 for regular users)
            if (avatarId < 1 || avatarId > 5)
            {
                return BadRequest("Avatar ID must be between 1 and 5.");
            }

            // Get the current user
            var (user, userId, error) = await GetCurrentUser();
            if (error != null) return error;
            var currentUser = user!; // Already checked for null above

            currentUser.AvatarId = avatarId;

            var result = await _userManager.UpdateAsync(currentUser);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Avatar change failed for {Username}", currentUser.UserName);
                return BadRequest(new
                {
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation("Avatar changed successfully for {Username}", currentUser.UserName);
            return Ok(new { Message = "Avatar changed successfully" });
        }

        // Helper method for getting the current user
        private async Task<(AuthUser? user, string? UserId, IActionResult? error)> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return (null, null, Unauthorized("No logged-in user."));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (null, null, NotFound("User not found."));

            return (user, userId, null);
        }
    }
}