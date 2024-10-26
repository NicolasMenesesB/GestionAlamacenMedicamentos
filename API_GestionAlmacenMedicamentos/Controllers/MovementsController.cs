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

        // GET: api/Movements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovementDTO>>> GetMovements()
        {
            return await _context.Movements
                .Include(m => m.TypeOfMovement)
                .Include(m => m.Batch)
                .Where(m => m.IsDeleted == "0")
                .Select(m => new MovementDTO
                {
                    MovementId = m.MovementId,
                    Quantity = m.Quantity,
                    DateOfMoviment = m.DateOfMoviment.ToString("yyyy-MM-dd"),
                    NameOfMovement = m.TypeOfMovement.NameOfMovement,
                    BatchCode = m.Batch.BatchCode
                })
                .ToListAsync();
        }

        // GET: api/Movements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovementDTO>> GetMovement(int id)
        {
            var movement = await _context.Movements
                .Include(m => m.TypeOfMovement)
                .Include(m => m.Batch)
                .Where(m => m.IsDeleted == "0" && m.MovementId == id)
                .FirstOrDefaultAsync();

            if (movement == null)
            {
                return NotFound();
            }

            var movementDTO = new MovementDTO
            {
                MovementId = movement.MovementId,
                Quantity = movement.Quantity,
                DateOfMoviment = movement.DateOfMoviment.ToString("yyyy-MM-dd"),
                NameOfMovement = movement.TypeOfMovement.NameOfMovement,
                BatchCode = movement.Batch.BatchCode
            };

            return movementDTO;
        }

        // POST: api/Movements
        [HttpPost]
        public async Task<ActionResult<MovementDTO>> PostMovement([FromBody] CreateMovementDTO createMovementDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscar el ID del tipo de movimiento por el nombre
            var typeOfMovement = await _context.TypeOfMovements
                .FirstOrDefaultAsync(t => t.NameOfMovement == createMovementDTO.NameOfMovement);

            if (typeOfMovement == null)
            {
                return BadRequest("Invalid TypeOfMovement");
            }

            // Buscar el lote por el código del lote
            var batch = await _context.Batches
                .FirstOrDefaultAsync(b => b.BatchCode == createMovementDTO.BatchCode);

            if (batch == null)
            {
                return BadRequest("Invalid BatchCode");
            }

            // Obtener el ID del usuario autenticado
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Crear el objeto Movement
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

            // Validar si la cantidad solicitada excede la cantidad actual en el lote
            if (createMovementDTO.Quantity > batch.CurrentQuantity)
            {
                return BadRequest("The quantity exceeds the available stock.");
            }

            // Actualizar la cantidad actual del lote
            if (typeOfMovement.NameOfMovement.StartsWith("Salida"))
            {
                batch.CurrentQuantity -= movement.Quantity;
                if (batch.CurrentQuantity <= batch.MinimumStock) // Alerta si llega al stock mínimo
                {
                    // Registrar el movimiento pero con una alerta sobre el stock mínimo
                    _context.Movements.Add(movement);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction("GetMovement", new { id = movement.MovementId },
                        new { Message = $"El lote {batch.BatchCode} ha alcanzado el stock mínimo.", Movement = movement });
                }
            }

            // Registrar el movimiento normalmente
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

            return CreatedAtAction("GetMovement", new { id = movement.MovementId }, movementDTO);
        }


        // PUT: api/Movements/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovement(int id, [FromBody] UpdateMovementDTO updateMovementDTO)
        {
            var movement = await _context.Movements
                .Include(m => m.TypeOfMovement)
                .Include(m => m.Batch)
                .Where(m => m.IsDeleted == "0" && m.MovementId == id)
                .FirstOrDefaultAsync();

            if (movement == null)
            {
                return NotFound();
            }

            // Buscar el tipo de movimiento por el nombre
            var typeOfMovement = await _context.TypeOfMovements
                .FirstOrDefaultAsync(t => t.NameOfMovement == updateMovementDTO.NameOfMovement);

            if (typeOfMovement == null)
            {
                return BadRequest("Invalid TypeOfMovement");
            }

            // Buscar el lote por el código del lote
            var batch = await _context.Batches
                .FirstOrDefaultAsync(b => b.BatchCode == updateMovementDTO.BatchCode);

            if (batch == null)
            {
                return BadRequest("Invalid BatchCode");
            }

            // Obtener el ID del usuario autenticado
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Revertir el cambio anterior en el lote
            if (movement.TypeOfMovement.NameOfMovement.StartsWith("Entrada"))
            {
                batch.CurrentQuantity -= movement.Quantity;
            }
            else if (movement.TypeOfMovement.NameOfMovement.StartsWith("Salida"))
            {
                batch.CurrentQuantity += movement.Quantity;
            }

            // Actualizar el movimiento y la cantidad en el lote
            movement.Quantity = updateMovementDTO.Quantity;
            movement.DateOfMoviment = DateOnly.Parse(updateMovementDTO.DateOfMoviment);
            movement.TypeOfMovementId = typeOfMovement.TypeOfMovementId;
            movement.BatchId = batch.BatchId;
            movement.UpdatedAt = DateTime.UtcNow;
            movement.UpdatedBy = userId;

            if (typeOfMovement.NameOfMovement.StartsWith("Entrada"))
            {
                batch.CurrentQuantity += movement.Quantity;
            }
            else if (typeOfMovement.NameOfMovement.StartsWith("Salida"))
            {
                batch.CurrentQuantity -= movement.Quantity;
                if (batch.CurrentQuantity < 0)
                {
                    return BadRequest("The quantity exceeds the available stock.");
                }
            }

            _context.Entry(movement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Concurrency error: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/Movements/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovement(int id)
        {
            var movement = await _context.Movements.FindAsync(id);
            if (movement == null)
            {
                return NotFound();
            }

            // Marcar como eliminado
            movement.IsDeleted = "1";
            movement.UpdatedAt = DateTime.UtcNow;

            _context.Entry(movement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Concurrency error: {ex.Message}");
            }

            return NoContent();
        }
    }
}
