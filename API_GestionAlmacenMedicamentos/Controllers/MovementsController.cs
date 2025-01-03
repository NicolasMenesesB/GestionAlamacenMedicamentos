using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs.MovementDTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MovementsController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;

        public MovementsController(Data.DbGestionAlmacenMedicamentosContext context)
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

        // GET: api/Movements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovementDTO>>> GetMovements([FromQuery] string? nameOfMovement = null)
        {
            var query = _context.Movements
                .Include(m => m.TypeOfMovement)
                .Include(m => m.Batch)
                    .ThenInclude(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                .Where(m => m.IsDeleted == "0");

            if (!string.IsNullOrEmpty(nameOfMovement))
            {
                query = query.Where(m => m.TypeOfMovement.NameOfMovement == nameOfMovement);
            }

            var movements = await query
                .Select(m => new MovementDTO
                {
                    MovementId = m.MovementId,
                    Quantity = m.Quantity,
                    DateOfMoviment = m.DateOfMoviment.ToString("yyyy-MM-dd"),
                    NameOfMovement = m.TypeOfMovement.NameOfMovement,
                    BatchCode = m.Batch.BatchCode
                })
                .ToListAsync();

            return Ok(movements);
        }

        // GET: api/Movements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovementDTO>> GetMovement(int id)
        {
            try
            {
                var movement = await _context.Movements
                    .Include(m => m.TypeOfMovement)
                    .Include(m => m.Batch)
                        .ThenInclude(b => b.MedicationHandlingUnit)
                            .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(m => m.MovementId == id && m.IsDeleted == "0");

                if (movement == null)
                {
                    return NotFound("Movimiento no encontrado.");
                }

                if (GetCurrentUserRole() != "0" && movement.Batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                var movementDTO = new MovementDTO
                {
                    MovementId = movement.MovementId,
                    Quantity = movement.Quantity,
                    DateOfMoviment = movement.DateOfMoviment.ToString("yyyy-MM-dd"),
                    NameOfMovement = movement.TypeOfMovement.NameOfMovement,
                    BatchCode = movement.Batch.BatchCode
                };

                return Ok(movementDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener el movimiento: {ex.Message}");
            }
        }

        // POST: api/Movements
        [HttpPost]
        public async Task<ActionResult<MovementDTO>> PostMovement([FromBody] CreateMovementDTO createMovementDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var typeOfMovement = await _context.TypeOfMovements
                    .FirstOrDefaultAsync(t => t.NameOfMovement == createMovementDTO.NameOfMovement);

                if (typeOfMovement == null)
                {
                    return BadRequest("Tipo de movimiento inválido.");
                }

                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(b => b.BatchCode == createMovementDTO.BatchCode);

                if (batch == null || batch.IsDeleted == "1")
                {
                    return BadRequest("Código de lote inválido.");
                }

                if (GetCurrentUserRole() != "0" && batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                var userId = GetCurrentUserId();

                if (typeOfMovement.NameOfMovement.StartsWith("Salida") && createMovementDTO.Quantity > batch.CurrentQuantity)
                {
                    return BadRequest("La cantidad excede el stock disponible.");
                }

                var movement = new Movement
                {
                    Quantity = createMovementDTO.Quantity,
                    DateOfMoviment = DateOnly.Parse(createMovementDTO.DateOfMoviment),
                    TypeOfMovementId = typeOfMovement.TypeOfMovementId,
                    BatchId = batch.BatchId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsDeleted = "0"
                };

                if (typeOfMovement.NameOfMovement.StartsWith("Salida"))
                {
                    batch.CurrentQuantity -= createMovementDTO.Quantity;
                }
                else if (typeOfMovement.NameOfMovement.StartsWith("Entrada"))
                {
                    batch.CurrentQuantity += createMovementDTO.Quantity;
                }

                _context.Movements.Add(movement);
                await _context.SaveChangesAsync();

                var movementDTO = new MovementDTO
                {
                    MovementId = movement.MovementId,
                    Quantity = movement.Quantity,
                    DateOfMoviment = movement.DateOfMoviment.ToString("yyyy-MM-dd"),
                    NameOfMovement = typeOfMovement.NameOfMovement,
                    BatchCode = batch.BatchCode
                };

                return CreatedAtAction(nameof(GetMovement), new { id = movement.MovementId }, movementDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al registrar el movimiento: {ex.Message}");
            }
        }

        // PUT: api/Movements/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovement(int id, [FromBody] UpdateMovementDTO updateMovementDTO)
        {
            var movement = await _context.Movements
                .Include(m => m.TypeOfMovement)
                .Include(m => m.Batch)
                    .ThenInclude(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                .Where(m => m.IsDeleted == "0" && m.MovementId == id)
                .FirstOrDefaultAsync();

            if (movement == null)
            {
                return NotFound("Movimiento no encontrado.");
            }

            if (GetCurrentUserRole() != "0" && movement.Batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
            {
                return Forbid("Acceso denegado: no tiene permisos para este almacén.");
            }

            try
            {
                var typeOfMovement = await _context.TypeOfMovements
                    .FirstOrDefaultAsync(t => t.NameOfMovement == updateMovementDTO.NameOfMovement);

                if (typeOfMovement == null)
                {
                    return BadRequest("Tipo de movimiento inválido.");
                }

                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(b => b.BatchCode == updateMovementDTO.BatchCode);

                if (batch == null || batch.IsDeleted == "1")
                {
                    return BadRequest("Código de lote inválido.");
                }

                // Revertir cambios anteriores
                if (movement.TypeOfMovement.NameOfMovement.StartsWith("Salida"))
                {
                    batch.CurrentQuantity += movement.Quantity;
                }
                else if (movement.TypeOfMovement.NameOfMovement.StartsWith("Entrada"))
                {
                    batch.CurrentQuantity -= movement.Quantity;
                }

                movement.Quantity = updateMovementDTO.Quantity;
                movement.DateOfMoviment = DateOnly.Parse(updateMovementDTO.DateOfMoviment);
                movement.TypeOfMovementId = typeOfMovement.TypeOfMovementId;
                movement.BatchId = batch.BatchId;
                movement.UpdatedAt = DateTime.UtcNow;
                movement.UpdatedBy = GetCurrentUserId();

                // Aplicar cambios nuevos
                if (typeOfMovement.NameOfMovement.StartsWith("Salida"))
                {
                    batch.CurrentQuantity -= updateMovementDTO.Quantity;
                    if (batch.CurrentQuantity < 0)
                    {
                        return BadRequest("La cantidad excede el stock disponible.");
                    }
                }
                else if (typeOfMovement.NameOfMovement.StartsWith("Entrada"))
                {
                    batch.CurrentQuantity += updateMovementDTO.Quantity;
                }

                _context.Entry(movement).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al actualizar el movimiento: {ex.Message}");
            }
        }

        // DELETE: api/Movements/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovement(int id)
        {
            try
            {
                // Buscar el movimiento junto con el lote y otros datos relacionados
                var movement = await _context.Movements
                    .Include(m => m.Batch)
                        .ThenInclude(b => b.MedicationHandlingUnit)
                            .ThenInclude(mhu => mhu.Shelf)
                    .Include(m => m.TypeOfMovement) // Asegúrate de incluir la relación con TypeOfMovement
                    .FirstOrDefaultAsync(m => m.MovementId == id && m.IsDeleted == "0");

                if (movement == null)
                {
                    // Devolver un resultado en formato JSON
                    return NotFound(new { success = false, message = "Movimiento no encontrado." });
                }

                // Verificar si el usuario tiene permisos para acceder al almacén
                if (GetCurrentUserRole() != "0" && movement.Batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Acceso denegado: no tiene permisos para este almacén." });
                }

                // Actualizar la cantidad actual del lote en función del tipo de movimiento
                if (movement.TypeOfMovement?.NameOfMovement?.StartsWith("Salida") == true)
                {
                    movement.Batch.CurrentQuantity += movement.Quantity;
                }
                else if (movement.TypeOfMovement?.NameOfMovement?.StartsWith("Entrada") == true)
                {
                    movement.Batch.CurrentQuantity -= movement.Quantity;

                    if (movement.Batch.CurrentQuantity < 0)
                    {
                        return BadRequest(new { success = false, message = "La cantidad actual del lote no puede ser negativa." });
                    }
                }
                else
                {
                    return BadRequest(new { success = false, message = "El tipo de movimiento no es válido o no está definido." });
                }

                // Marcar el movimiento como eliminado lógicamente
                movement.IsDeleted = "1";
                movement.UpdatedAt = DateTime.UtcNow;

                // Guardar los cambios en la base de datos
                _context.Entry(movement).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Manejar errores inesperados
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error al eliminar el movimiento: {ex.Message}" });
            }
        }

        // RESTORE: api/Movements/restore/5
        [HttpPost("restore/{id}")]
        public async Task<IActionResult> RestoreMovement(int id)
        {
            try
            {
                // Buscar el movimiento eliminado junto con los datos necesarios
                var movement = await _context.Movements
                    .Include(m => m.Batch)
                        .ThenInclude(b => b.MedicationHandlingUnit)
                            .ThenInclude(mhu => mhu.Shelf)
                    .Include(m => m.TypeOfMovement)
                    .FirstOrDefaultAsync(m => m.MovementId == id && m.IsDeleted == "1");

                if (movement == null)
                {
                    return NotFound(new { success = false, message = "Movimiento no encontrado o ya está activo." });
                }

                // Verificar permisos del usuario
                if (GetCurrentUserRole() != "0" && movement.Batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Acceso denegado: no tiene permisos para este almacén." });
                }

                // Restablecer el movimiento
                movement.IsDeleted = "0";
                movement.UpdatedAt = DateTime.UtcNow;

                // Ajustar la cantidad del lote según el tipo de movimiento
                if (movement.TypeOfMovement.NameOfMovement.StartsWith("Salida"))
                {
                    movement.Batch.CurrentQuantity -= movement.Quantity;

                    if (movement.Batch.CurrentQuantity < 0)
                    {
                        return BadRequest(new { success = false, message = "La cantidad actual del lote no puede ser negativa." });
                    }
                }
                else if (movement.TypeOfMovement.NameOfMovement.StartsWith("Entrada"))
                {
                    movement.Batch.CurrentQuantity += movement.Quantity;
                }
                else
                {
                    return BadRequest(new { success = false, message = "El tipo de movimiento no es válido o no está definido." });
                }

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Movimiento restaurado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error al restaurar el movimiento: {ex.Message}" });
            }
        }

        // GET: api/Movements/deleted
        [HttpGet("deleted")]
        public async Task<ActionResult<IEnumerable<MovementDTO>>> GetDeletedMovements()
        {
            try
            {
                var deletedMovements = await _context.Movements
                    .Include(m => m.TypeOfMovement)
                    .Include(m => m.Batch)
                    .Where(m => m.IsDeleted == "1")
                    .Select(movement => new MovementDTO
                    {
                        MovementId = movement.MovementId,
                        Quantity = movement.Quantity,
                        DateOfMoviment = movement.DateOfMoviment.ToString("yyyy-MM-dd"),
                        NameOfMovement = movement.TypeOfMovement.NameOfMovement,
                        BatchCode = movement.Batch.BatchCode
                    })
                    .ToListAsync();

                return Ok(deletedMovements);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error al obtener los movimientos eliminados: {ex.Message}" });
            }
        }

    }
}
