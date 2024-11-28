using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs.ShelfDTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ShelvesController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;

        public ShelvesController(Data.DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetCurrentUserRole()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private int? GetCurrentWarehouseId()
        {
            var warehouseId = User.Claims.FirstOrDefault(c => c.Type == "WarehouseId")?.Value;
            return string.IsNullOrEmpty(warehouseId) ? null : int.Parse(warehouseId);
        }

        // GET: api/Shelves
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfDTO>>> GetShelves()
        {
            try
            {
                // Obtener el ID del almacén actual desde el token
                var currentWarehouseId = GetCurrentWarehouseId();

                if (currentWarehouseId == null && GetCurrentUserRole() != "0") // Solo Admin puede ver todos
                {
                    return Forbid("Acceso denegado: no se puede determinar el almacén del usuario.");
                }

                var shelves = await _context.Shelves
                    .Where(s => s.IsDeleted == "0" && (s.WarehouseId == currentWarehouseId || GetCurrentUserRole() == "0"))
                    .Select(s => new ShelfDTO
                    {
                        ShelfId = s.ShelfId,
                        NameShelf = s.NameShelf,
                        WarehouseName = _context.Warehouses
                            .Where(w => w.WarehouseId == s.WarehouseId)
                            .Select(w => w.NameWarehouse)
                            .FirstOrDefault() ?? "N/A"
                    })
                    .ToListAsync();

                return Ok(shelves);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener los estantes: {ex.Message}");
            }
        }

        // GET: api/Shelves/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShelfDTO>> GetShelf(int id)
        {
            try
            {
                var shelf = await _context.Shelves.FindAsync(id);

                if (shelf == null || shelf.IsDeleted == "1")
                {
                    return NotFound("Estante no encontrado.");
                }

                // Validar si el usuario tiene acceso al almacén del estante
                if (GetCurrentUserRole() != "0" && shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                var shelfDTO = new ShelfDTO
                {
                    ShelfId = shelf.ShelfId,
                    NameShelf = shelf.NameShelf,
                    WarehouseName = _context.Warehouses.FirstOrDefault(w => w.WarehouseId == shelf.WarehouseId)?.NameWarehouse
                };

                return Ok(shelfDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener el estante: {ex.Message}");
            }
        }

        // POST: api/Shelves
        [HttpPost]
        public async Task<ActionResult<ShelfDTO>> PostShelf([FromBody] CreateShelfDTO createShelfDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Buscar el almacén por nombre
                var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.NameWarehouse == createShelfDTO.WarehouseName);

                if (warehouse == null)
                {
                    return BadRequest("El almacén proporcionado no existe.");
                }

                // Validar si el usuario tiene acceso al almacén
                if (GetCurrentUserRole() != "0" && warehouse.WarehouseId != GetCurrentWarehouseId())
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                var userId = GetCurrentUserId();

                var shelf = new Shelf
                {
                    NameShelf = createShelfDTO.NameShelf,
                    WarehouseId = warehouse.WarehouseId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsDeleted = "0"
                };

                _context.Shelves.Add(shelf);
                await _context.SaveChangesAsync();

                var shelfDTO = new ShelfDTO
                {
                    ShelfId = shelf.ShelfId,
                    NameShelf = shelf.NameShelf,
                    WarehouseName = warehouse.NameWarehouse
                };

                return CreatedAtAction(nameof(GetShelf), new { id = shelf.ShelfId }, shelfDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear el estante: {ex.Message}");
            }
        }

        // PUT: api/Shelves/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShelf(int id, [FromBody] UpdateShelfDTO updateShelfDTO)
        {
            try
            {
                var shelf = await _context.Shelves.FindAsync(id);

                if (shelf == null || shelf.IsDeleted == "1")
                {
                    return NotFound("Estante no encontrado.");
                }

                var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.NameWarehouse == updateShelfDTO.WarehouseName);

                if (warehouse == null)
                {
                    return BadRequest("El almacén proporcionado no existe.");
                }

                // Validar si el usuario tiene acceso al almacén
                if (GetCurrentUserRole() != "0" && warehouse.WarehouseId != GetCurrentWarehouseId())
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                var userId = GetCurrentUserId();

                shelf.NameShelf = updateShelfDTO.NameShelf;
                shelf.WarehouseId = warehouse.WarehouseId;
                shelf.UpdatedAt = DateTime.UtcNow;
                shelf.UpdatedBy = userId;

                _context.Entry(shelf).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al actualizar el estante: {ex.Message}");
            }
        }

        // DELETE: api/Shelves/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShelf(int id)
        {
            try
            {
                var shelf = await _context.Shelves.FindAsync(id);

                if (shelf == null || shelf.IsDeleted == "1")
                {
                    return NotFound("Estante no encontrado.");
                }

                // Validar si el usuario tiene acceso al almacén
                if (GetCurrentUserRole() != "0" && shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                shelf.IsDeleted = "1";
                shelf.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al eliminar el estante: {ex.Message}");
            }
        }

        private bool ShelfExists(int id)
        {
            return _context.Shelves.Any(e => e.ShelfId == id && e.IsDeleted == "0");
        }
    }
}
