using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicketManagementSystem.Models;

namespace TicketManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserContext _userContext;

        public AuthController(IConfiguration config, UserContext userContext)
        {
            _config = config;
            _userContext = userContext;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            User? user = _userContext.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User does not exist.");
            }

            if (request.Password == user.Password)
            {
                var token = GenerateJwtToken(user.Email, user.Id, user.Role);
                return Ok(new { token });
            }

            return Unauthorized();
        }

        private string GenerateJwtToken(string email, Guid userId, Role role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("role", role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
