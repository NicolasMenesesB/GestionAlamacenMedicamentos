using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs.UserDTOs;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public UsersController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            return await _context.Users
                .Where(u => u.IsDeleted == "0")
                .Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Role = u.Role
                })
                .ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.IsDeleted == "1")
            {
                return NotFound();
            }

            var userDTO = new UserDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Role = user.Role
            };

            return userDTO;
        }
    }
}
