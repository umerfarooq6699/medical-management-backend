using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MMGC_Project.Models;


namespace MMGC_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager   = userManager;
            _roleManager   = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // 1. Check if user already exists
            var userExists = await _userManager.FindByEmailAsync(model.EmailAddress);
            if (userExists != null)
                return BadRequest(new { Status = "Error", Message = "User already exists!" });

            // 2. Create the user object
            ApplicationUser user = new()
            {
                Email = model.EmailAddress,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.EmailAddress,
                FullName = model.FullName,
                Contact = model.ContactNumber,
                Gender = model.Gender,
                PhoneNumber = model.ContactNumber,
                Role = "patient"
            };

            // 3. Save user to database
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { Status = "Error", Message = "User creation failed!", Errors = result.Errors });

            // 4. Handle Role assignment
            string assignedRole = "Patient";

            if (!await _roleManager.RoleExistsAsync(assignedRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(assignedRole));
            }

            await _userManager.AddToRoleAsync(user, assignedRole);

            return Ok(new { Status = "Success", Message = "User created successfully!" });
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // 1. Find user by Name (FullName)
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.FullName == model.Name);

            if (user == null)
                return Unauthorized(new { Status = "Error", Message = "Invalid name or password." });

            // 2. Verify password
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { Status = "Error", Message = "Invalid name or password." });

            // 3. Build claims (info embedded in the token)
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FullName",       user.FullName),
                new Claim(ClaimTypes.Role,  user.Role), // Read directly from the Users table column
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // 4. Generate JWT token
            var authSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            var token = new JwtSecurityToken(
                issuer:             _configuration["JWT:ValidIssuer"],
                audience:           _configuration["JWT:ValidAudience"],
                expires:            DateTime.UtcNow.AddHours(8),
                claims:             authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                Status      = "Success",
                Token       = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration  = token.ValidTo,
                User = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Contact,
                    user.Gender,
                    Roles = new[] { user.Role } // Use the direct Role column from the database!
                }
            });
        }

        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Contact,
                    u.Gender,
                    u.UserName,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                    return Unauthorized(new { Status = "Error", Message = "User is not authenticated." });

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return NotFound(new { Status = "Error", Message = "User not found." });

                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new { Status = "Error", Message = "Password change failed.", Errors = errors });
                }

                return Ok(new { Status = "Success", Message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = ex.Message, Detailed = ex.ToString() });
            }
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                    return Unauthorized(new { Status = "Error", Message = "User is not authenticated." });

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return NotFound(new { Status = "Error", Message = "User not found." });

                // Check email uniqueness if it is changing
                if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _userManager.FindByEmailAsync(model.Email);
                    if (emailExists != null)
                        return BadRequest(new { Status = "Error", Message = "Email address is already in use." });
                }

                // Check full name uniqueness if it is changing
                if (!string.Equals(user.FullName, model.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var nameExists = await _userManager.Users.AnyAsync(u => u.FullName == model.Name && u.Id != user.Id);
                    if (nameExists)
                        return BadRequest(new { Status = "Error", Message = "Full Name is already in use." });
                }

                // Update properties
                user.FullName = model.Name;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.Contact = model.Contact;
                user.PhoneNumber = model.Contact;
                user.Gender = model.Gender;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new { Status = "Error", Message = "Profile update failed.", Errors = errors });
                }

                // Re-generate JWT claims with updated info
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,  user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim("FullName",       user.FullName),
                    new Claim(ClaimTypes.Role,  user.Role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var authSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

                var token = new JwtSecurityToken(
                    issuer:             _configuration["JWT:ValidIssuer"],
                    audience:           _configuration["JWT:ValidAudience"],
                    expires:            DateTime.UtcNow.AddHours(8),
                    claims:             authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    Status      = "Success",
                    Message     = "Profile updated successfully.",
                    Token       = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration  = token.ValidTo,
                    User = new
                    {
                        user.Id,
                        user.FullName,
                        user.Email,
                        user.Contact,
                        user.Gender,
                        Roles = new[] { user.Role }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = ex.Message, Detailed = ex.ToString() });
            }
        }
    }
}
