using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.DTOs.WarehouseDTOs;
using API_GestionAlmacenMedicamentos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;
using System.Security.Claims;


namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class WarehousesController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;

        public WarehousesController(Data.DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/Warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseDTO>>> GetWarehouses()
        {
            return await _context.Warehouses
                .Where(w => w.IsDeleted == "0")
                .Select(w => new WarehouseDTO
                {
                    WarehouseId = w.WarehouseId,
                    NameWarehouse = w.NameWarehouse,
                    AddressWarehouse = w.AddressWarehouse
                })
                .ToListAsync();
        }

        // GET: api/Warehouses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WarehouseDTO>> GetWarehouse(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);

            if (warehouse == null || warehouse.IsDeleted == "1")
            {
                return NotFound();
            }

            var warehouseDTO = new WarehouseDTO
            {
                WarehouseId = warehouse.WarehouseId,
                NameWarehouse = warehouse.NameWarehouse,
                AddressWarehouse = warehouse.AddressWarehouse
            };

            return warehouseDTO;
        }

        // POST: api/Warehouses
        [HttpPost]
        public async Task<ActionResult<WarehouseDTO>> PostWarehouse([FromBody] CreateWarehouseDTO createWarehouseDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si el Claim "name" existe antes de intentar obtener el userId
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return Unauthorized("User information is missing.");
            }

            // Obtener el userId desde el JWT
            if (!int.TryParse(claim.Value, out var userId))
            {
                return Unauthorized("Invalid user information.");
            }

            var warehouse = new Warehouse
            {
                NameWarehouse = createWarehouseDTO.NameWarehouse,
                AddressWarehouse = createWarehouseDTO.AddressWarehouse,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,  // Asignar el ID del usuario autenticado
                IsDeleted = "0"
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            var warehouseDTO = new WarehouseDTO
            {
                WarehouseId = warehouse.WarehouseId,
                NameWarehouse = warehouse.NameWarehouse,
                AddressWarehouse = warehouse.AddressWarehouse
            };

            return CreatedAtAction("GetWarehouse", new { id = warehouse.WarehouseId }, warehouseDTO);
        }


        // PUT: api/Warehouses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWarehouse(int id, [FromBody] UpdateWarehouseDTO updateWarehouseDTO)
        {
            // Verificar si el almacén existe y no está marcado como eliminado
            var warehouse = await _context.Warehouses.FindAsync(id);

            if (warehouse == null || warehouse.IsDeleted == "1")
            {
                return NotFound();
            }

            // Verificar si el Claim "name" existe antes de intentar obtener el userId
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return Unauthorized("User information is missing.");
            }

            // Obtener el userId desde el JWT
            if (!int.TryParse(claim.Value, out var userId))
            {
                return Unauthorized("Invalid user information.");
            }

            // Actualizar los datos del almacén
            warehouse.NameWarehouse = updateWarehouseDTO.NameWarehouse;
            warehouse.AddressWarehouse = updateWarehouseDTO.AddressWarehouse;
            warehouse.UpdatedAt = DateTime.UtcNow;
            warehouse.UpdatedBy = userId;  // Asignar el ID del usuario autenticado

            _context.Entry(warehouse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Concurrency error: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"SQL type error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message} - {ex.InnerException?.Message}");
            }

            return NoContent();
        }


        // DELETE: api/Warehouses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                return NotFound();
            }

            warehouse.IsDeleted = "1";
            warehouse.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Concurrency error: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"SQL type error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message} - {ex.InnerException?.Message}");
            }

            return NoContent();
        }

        [HttpGet("CheckWarehouseExists")]
        public IActionResult CheckWarehouseExists(string nameWarehouse)
        {
            var warehouseExists = _context.Warehouses
                .Any(w => w.NameWarehouse.ToLower() == nameWarehouse.ToLower() && w.IsDeleted == "0");

            return Ok(new { warehouseExists });
        }

        private bool WarehouseExists(int id)
        {
            return _context.Warehouses.Any(e => e.WarehouseId == id && e.IsDeleted == "0");
        }
    }
}
