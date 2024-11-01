using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserWarehouseController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;

        public UserWarehouseController(Data.DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/UserWarehouse
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUserWarehouses()
        {
            var userWarehouses = await _context.UserWarehouses
                .Where(uw => uw.IsDeleted == "0")
                .Select(uw => new
                {
                    uw.UserWarehouseId,
                    UserName = uw.User.UserName,
                    WarehouseName = uw.Warehouse.NameWarehouse,
                    uw.CreatedAt
                })
                .ToListAsync();

            return Ok(userWarehouses);
        }

        // GET: api/UserWarehouse/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUserWarehouse(int id)
        {
            var userWarehouse = await _context.UserWarehouses
                .Where(uw => uw.UserWarehouseId == id && uw.IsDeleted == "0")
                .Select(uw => new
                {
                    uw.UserWarehouseId,
                    UserName = uw.User.UserName,
                    WarehouseName = uw.Warehouse.NameWarehouse,
                    uw.CreatedAt,
                    uw.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (userWarehouse == null)
            {
                return NotFound();
            }

            return Ok(userWarehouse);
        }

        // POST: api/UserWarehouse
        [HttpPost("assign")]
        public async Task<IActionResult> AssignUserToWarehouse(int userId, int warehouseId)
        {
            // Verificar si el usuario existe
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "El usuario no existe." });
            }

            // Verificar si el almacén existe
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
            if (warehouse == null)
            {
                return NotFound(new { Message = "El almacén no existe." });
            }

            // Verificar el rol del usuario
            if (user.Role == "2") // Empleado de almacén
            {
                // Verificar si el empleado ya está asignado a un almacén
                var existingAssignment = await _context.UserWarehouses
                    .AnyAsync(uw => uw.UserId == userId);

                if (existingAssignment)
                {
                    return BadRequest(new { Message = "Un empleado de almacén solo puede estar asignado a un único almacén." });
                }
            }

            // Asignación para cualquier usuario (administrador, jefe de almacén, empleado)
            var userWarehouse = new UserWarehouse
            {
                UserId = userId,
                WarehouseId = warehouseId,
                CreatedAt = DateTime.Now,
                CreatedBy = 8, // Supongamos que el administrador es el creador
                IsDeleted = "0"
            };

            _context.UserWarehouses.Add(userWarehouse);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Usuario asignado al almacén exitosamente." });
        }

        // DELETE: api/UserWarehouse/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserWarehouse(int id)
        {
            var userWarehouse = await _context.UserWarehouses.FindAsync(id);
            if (userWarehouse == null)
            {
                return NotFound();
            }

            userWarehouse.IsDeleted = "1";
            userWarehouse.UpdatedAt = DateTime.UtcNow;
            userWarehouse.UpdatedBy = 8;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Verificar si existe una asignación
        private bool UserWarehouseExists(int id)
        {
            return _context.UserWarehouses.Any(e => e.UserWarehouseId == id && e.IsDeleted == "0");
        }
    }
}
