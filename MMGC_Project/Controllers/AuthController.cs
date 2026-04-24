using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest(new { Status = "Error", Message = "User already exists!" });

            // 2. Create the user object
            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                FullName = model.Name
            };

            // 3. Save user to database
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { Status = "Error", Message = "User creation failed!", Errors = result.Errors });

            // 4. Handle Role assignment
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok(new { Status = "Success", Message = "User created successfully!" });
        }

        // ── POST /api/auth/login ──────────────────────────────────────────────
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
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FullName",       user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in userRoles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));

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
                    Roles = userRoles
                }
            });
        }
    }
}
