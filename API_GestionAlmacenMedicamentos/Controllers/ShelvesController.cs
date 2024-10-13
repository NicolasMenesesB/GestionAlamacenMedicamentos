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
using System.Data.SqlTypes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ShelvesController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public ShelvesController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/Shelves
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfDTO>>> GetShelves()
        {
            return await _context.Shelves
                 .Where(s => s.IsDeleted == "0")
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
        }

        // GET: api/Shelves/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShelfDTO>> GetShelf(int id)
        {
            var shelf = await _context.Shelves.FindAsync(id);

            if (shelf == null || shelf.IsDeleted == "1")
            {
                return NotFound();
            }

            var shelfDTO = new ShelfDTO
            {
                ShelfId = shelf.ShelfId,
                NameShelf = shelf.NameShelf,
                WarehouseName = _context.Warehouses.FirstOrDefault(w => w.WarehouseId == shelf.WarehouseId)?.NameWarehouse  
            };

            return shelfDTO;
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
                // Buscar el ID del almacén usando el nombre
                var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.NameWarehouse == createShelfDTO.WarehouseName);

                if (warehouse == null)
                {
                    return BadRequest("El almacén proporcionado no existe.");
                }

                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

                var shelf = new Shelf
                {
                    NameShelf = createShelfDTO.NameShelf,
                    WarehouseId = warehouse.WarehouseId,  // Asignar el ID del almacén
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

                return CreatedAtAction("GetShelf", new { id = shelf.ShelfId }, shelfDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear el estante: {ex.Message} - {ex.InnerException?.Message}");
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
                    return NotFound();
                }

                // Buscar el ID del almacén usando el nombre
                var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.NameWarehouse == updateShelfDTO.WarehouseName);

                if (warehouse == null)
                {
                    return BadRequest("El almacén proporcionado no existe.");
                }

                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

                shelf.NameShelf = updateShelfDTO.NameShelf;
                shelf.WarehouseId = warehouse.WarehouseId;  // Asignar el ID del almacén
                shelf.UpdatedAt = DateTime.UtcNow;
                shelf.UpdatedBy = userId;

                _context.Entry(shelf).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error de concurrencia al actualizar el estante: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Error de tipo SQL al actualizar el estante: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al actualizar el estante: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // DELETE: api/Shelves/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShelf(int id)
        {
            try
            {
                var shelf = await _context.Shelves.FindAsync(id);
                if (shelf == null)
                {
                    return NotFound();
                }

                shelf.IsDeleted = "1";
                shelf.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error de concurrencia al eliminar el estante: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Error de tipo SQL al eliminar el estante: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar el estante: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // Verificar si existe un estante
        [HttpGet("CheckShelfExists")]
        public IActionResult CheckShelfExists(string nameShelf)
        {
            var shelfExists = _context.Shelves
                .Any(s => s.NameShelf.ToLower() == nameShelf.ToLower() && s.IsDeleted == "0");

            return Ok(new { shelfExists });
        }

        private bool ShelfExists(int id)
        {
            return _context.Shelves.Any(e => e.ShelfId == id && e.IsDeleted == "0");
        }
    }
}
