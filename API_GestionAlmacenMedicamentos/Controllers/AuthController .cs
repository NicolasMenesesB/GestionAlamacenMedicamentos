using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using BCrypt.Net;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(Data.DbGestionAlmacenMedicamentosContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Modelo de login inválido.");
                return BadRequest("Modelo de login inválido.");
            }

            // Buscar usuario en la base de datos
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == login.UserName);

            if (user == null)
            {
                Console.WriteLine($"Usuario no encontrado: {login.UserName}");
                return Unauthorized("Usuario o contraseña incorrectos.");
            }

            // Verificar la contraseña
            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
            {
                Console.WriteLine($"Contraseña incorrecta para el usuario: {login.UserName}");
                return Unauthorized("Usuario o contraseña incorrectos.");
            }

            // Buscar el WarehouseId del usuario (solo si no es administrador)
            string warehouseId = null;
            if (user.Role != "0") // Si no es administrador
            {
                warehouseId = await _context.UserWarehouses
                    .Where(uw => uw.UserId == user.UserId && uw.IsDeleted == "0")
                    .Select(uw => uw.WarehouseId.ToString())
                    .FirstOrDefaultAsync();

                // Asegúrate de que el usuario tiene un WarehouseId asignado
                if (string.IsNullOrEmpty(warehouseId))
                {
                    return Unauthorized("El usuario no tiene un almacén asignado.");
                }
            }

            // Generar token JWT
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Role, user.Role) 
                    }),
                    Expires = DateTime.UtcNow.AddHours(1), 
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                // Agregar el WarehouseId al token solo si no es administrador
                if (user.Role != "0")
                {
                    tokenDescriptor.Subject.AddClaim(new Claim("WarehouseId", warehouseId));
                }

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                Console.WriteLine($"Login exitoso para el usuario: {login.UserName}");
                return Ok(new { Token = tokenString });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar el token para el usuario {login.UserName}: {ex.Message}");
                return StatusCode(500, "Error al generar el token de autenticación.");
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Console.WriteLine("Sesión cerrada.");
            return NoContent();
        }
    }
}
