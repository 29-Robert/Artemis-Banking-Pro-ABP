using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IGenericRepository<User> userRepository,
        IConfiguration configuration) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var users = await userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { Mensaje = "Credenciales inválidas." });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { Mensaje = "Su cuenta se encuentra inactiva. Revise su correo." });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, Usuario = user.Username, Rol = ObtenerNombreDeRol(user.RoleId) });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, ObtenerNombreDeRol(user.RoleId))
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["DurationInHours"] ?? "2")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private static string ObtenerNombreDeRol(int roleId)
        {
            return roleId switch
            {
                1 => "Administrador",
                2 => "Cajero",
                3 => "Cliente",
                4 => "Comercio",
                _ => "Usuario"
            };
        }
    }
}