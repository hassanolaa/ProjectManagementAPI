using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManagementAPI.Helpers;
using TaskManagementAPI.Models.DTOs.Auth;
using TaskManagementAPI.Models.Entities;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IEmailSender emailSender,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailSender = emailSender;
            _logger = logger;
        }

//         [HttpPost("test-email")]

// public async Task<IActionResult> TestEmail()
// {
//     try
//     {
//         await _emailSender.SendEmailAsync(
//             "hssanabdl975@gmail.com",
//             "Test Email",
//             "This is a test email from Task Management API");
            
//         return Ok(new { message = "Email sent successfully" });
//     }
//     catch (Exception ex)
//     {
//         return BadRequest(new { message = $"Email failed: {ex.Message}" });
//     }
// }

        /// <summary>
        /// Register a new user account
        /// </summary>
        [HttpPost("register")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
     {       
    try
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return BadRequest(MappingHelper.CreateErrorResponse(
                "User with this email already exists.", 
                new List<string> { "Email is already registered." }));
        }

        // Create new user
        var user = MappingHelper.MapToApplicationUser(dto);
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(MappingHelper.CreateErrorResponse(
                "Registration failed.", result.Errors));
        }

        // Add default role
        await _userManager.AddToRoleAsync(user, "User");

        // For development - auto-confirm email
        user.EmailConfirmed = true;
        user.LastActiveAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        
        
        // Generate JWT token (auto-login)
        var token = await _tokenService.GenerateJwtTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var userProfile = MappingHelper.MapToUserProfileDto(user, roles);
        
        // Comment out email sending for now
        await _emailSender.SendWelcomeEmailAsync(user.Email!, $"{user.FirstName} {user.LastName}");

               _logger.LogInformation("User {Email} registered and logged in successfully", dto.Email);

        return Ok(MappingHelper.CreateSuccessResponse(
            "Registration successful. You are now logged in.", token, userProfile));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during user registration for {Email}", dto.Email);
        return StatusCode(500, MappingHelper.CreateErrorResponse(
            "An error occurred during registration."));
    }
}
        // public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        // {
        //     try
        //     {
        //         // Check if user already exists
        //         var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        //         if (existingUser != null)
        //         {
        //             return BadRequest(MappingHelper.CreateErrorResponse(
        //                 "User with this email already exists.", 
        //                 new List<string> { "Email is already registered." }));
        //         }

        //         // Create new user
        //         var user = MappingHelper.MapToApplicationUser(dto);
        //         var result = await _userManager.CreateAsync(user, dto.Password);

        //         if (!result.Succeeded)
        //         {
        //             return BadRequest(MappingHelper.CreateErrorResponse(
        //                 "Registration failed.", result.Errors));
        //         }

        //         // Add default role
        //         await _userManager.AddToRoleAsync(user, "User");

        //         // Generate email confirmation token
        //         var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //         var confirmationLink = Url.Action(
        //             "ConfirmEmail", 
        //             "Auth", 
        //             new { userId = user.Id, token = emailToken }, 
        //             Request.Scheme);

        //         // Send welcome email with confirmation link
        //         await _emailSender.SendWelcomeEmailAsync(user.Email!, $"{user.FirstName} {user.LastName}");

        //         _logger.LogInformation("User {Email} registered successfully", dto.Email);

        //         return Ok(MappingHelper.CreateSuccessResponse(
        //             "Registration successful. Please check your email to confirm your account."));
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error during user registration for {Email}", dto.Email);
        //         return StatusCode(500, MappingHelper.CreateErrorResponse(
        //             "An error occurred during registration."));
        //     }
        // }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return Unauthorized(MappingHelper.CreateErrorResponse(
                        "Invalid email or password."));
                }

                // Check if email is confirmed
                if (!user.EmailConfirmed)
                {
                    return Unauthorized(MappingHelper.CreateErrorResponse(
                        "Email not confirmed. Please check your email and confirm your account."));
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return Unauthorized(MappingHelper.CreateErrorResponse(
                        "Account is deactivated. Please contact support."));
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!, 
                    dto.Password, 
                    dto.RememberMe, 
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    // Update last active time
                    user.LastActiveAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Generate JWT token
                    var token = await _tokenService.GenerateJwtTokenAsync(user);
                    var roles = await _userManager.GetRolesAsync(user);
                    var userProfile = MappingHelper.MapToUserProfileDto(user, roles);

                    _logger.LogInformation("User {Email} logged in successfully", dto.Email);

                    return Ok(MappingHelper.CreateSuccessResponse(
                        "Login successful.", token, userProfile));
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} account is locked out", dto.Email);
                    return Unauthorized(MappingHelper.CreateErrorResponse(
                        "Account is locked due to multiple failed login attempts. Please try again later."));
                }

                if (result.IsNotAllowed)
                {
                    return Unauthorized(MappingHelper.CreateErrorResponse(
                        "Login not allowed. Please confirm your email address."));
                }

                _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
                return Unauthorized(MappingHelper.CreateErrorResponse(
                    "Invalid email or password."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", dto.Email);
                return StatusCode(500, MappingHelper.CreateErrorResponse(
                    "An error occurred during login."));
            }
        }

        /// <summary>
        /// Request password reset email
        /// </summary>
        [HttpPost("forgot-password")]
        [EnableRateLimiting("PasswordResetPolicy")]
        public async Task<ActionResult<AuthResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                
                // Always return success to prevent email enumeration
                // but only send email if user exists
                if (user != null && user.EmailConfirmed)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetLink = Url.Action(
                        "ResetPassword", 
                        "Auth", 
                        new { email = user.Email, token = token }, 
                        Request.Scheme);

                    await _emailSender.SendPasswordResetEmailAsync(
                        user.Email!, 
                        resetLink!, 
                        $"{user.FirstName} {user.LastName}");

                    _logger.LogInformation("Password reset email sent to {Email}", dto.Email);
                }
                else
                {
                    _logger.LogWarning("Password reset requested for non-existent or unconfirmed email: {Email}", dto.Email);
                }

                return Ok(MappingHelper.CreateSuccessResponse(
                    "If the email address exists in our system, you will receive a password reset link."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for {Email}", dto.Email);
                return StatusCode(500, MappingHelper.CreateErrorResponse(
                    "An error occurred while processing your request."));
            }
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        [HttpPost("reset-password")]
        [EnableRateLimiting("PasswordResetPolicy")]
        public async Task<ActionResult<AuthResponseDto>> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return BadRequest(MappingHelper.CreateErrorResponse(
                        "Invalid password reset request."));
                }

                var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
                
                if (result.Succeeded)
                {
                    // Reset lockout count
                    await _userManager.ResetAccessFailedCountAsync(user);
                    
                    _logger.LogInformation("Password reset successful for {Email}", dto.Email);
                    
                    return Ok(MappingHelper.CreateSuccessResponse(
                        "Password has been reset successfully. You can now login with your new password."));
                }

                return BadRequest(MappingHelper.CreateErrorResponse(
                    "Password reset failed.", result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", dto.Email);
                return StatusCode(500, MappingHelper.CreateErrorResponse(
                    "An error occurred while resetting your password."));
            }
        }

        /// <summary>
        /// Confirm email address
        /// </summary>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid email confirmation request.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Invalid user.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmed for user {UserId}", userId);
                return Ok("Email confirmed successfully. You can now login to your account.");
            }

            return BadRequest("Email confirmation failed.");
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
     [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
{
    try
    {
        // Fix: Use the correct claim types that are actually in your JWT token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                   ?? User.FindFirst("sub")?.Value 
                   ?? User.FindFirst("id")?.Value;
                   
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("No user ID found in token claims");
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userProfile = MappingHelper.MapToUserProfileDto(user, roles);

        return Ok(userProfile);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving user profile");
        return StatusCode(500, "An error occurred while retrieving your profile.");
    }
}


        /// <summary>
        /// Logout (invalidate token - client-side responsibility)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // In a JWT-based system, logout is primarily handled on the client side
                // by removing the token. However, you could implement token blacklisting here
                // if needed for additional security.
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("sub")?.Value 
                           ?? User.FindFirst("id")?.Value;
                _logger.LogInformation("User {UserId} logged out", userId);

                return Ok(new { message = "Logout successful." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "An error occurred during logout.");
            }
        }
    }
}
