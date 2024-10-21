using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs.TypeOfMovementDTOs;
using System.Data.SqlTypes;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TypeOfMovementsController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;

        public TypeOfMovementsController(Data.DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/TypeOfMovements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeOfMovementDTO>>> GetTypeOfMovements()
        {
            return await _context.TypeOfMovements
                 .Where(tm => tm.IsDeleted == "0")
                 .Select(tm => new TypeOfMovementDTO
                 {
                     TypeOfMovementId = tm.TypeOfMovementId,
                     NameOfMovement = tm.NameOfMovement,
                     DescriptionOfMovement = tm.DescriptionOfMovement
                 })
                 .ToListAsync();
        }

        // GET: api/TypeOfMovements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeOfMovementDTO>> GetTypeOfMovement(int id)
        {
            var typeOfMovement = await _context.TypeOfMovements.FindAsync(id);

            if (typeOfMovement == null || typeOfMovement.IsDeleted == "1")
            {
                return NotFound();
            }

            var typeOfMovementDTO = new TypeOfMovementDTO
            {
                TypeOfMovementId = typeOfMovement.TypeOfMovementId,
                NameOfMovement = typeOfMovement.NameOfMovement,
                DescriptionOfMovement = typeOfMovement.DescriptionOfMovement
            };

            return typeOfMovementDTO;
        }

        [HttpPost]
        public async Task<ActionResult<TypeOfMovementDTO>> PostTypeOfMovement([FromBody] CreateTypeOfMovementDTO createTypeOfMovementDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si la reclamación "name" existe
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return Unauthorized("User information is missing.");
            }

            // Obtener el userId desde el JWT
            var userId = int.Parse(claim.Value);

            var typeOfMovement = new TypeOfMovement
            {
                NameOfMovement = createTypeOfMovementDTO.NameOfMovement,
                DescriptionOfMovement = createTypeOfMovementDTO.DescriptionOfMovement,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsDeleted = "0"
            };

            _context.TypeOfMovements.Add(typeOfMovement);
            await _context.SaveChangesAsync();

            var typeOfMovementDTO = new TypeOfMovementDTO
            {
                TypeOfMovementId = typeOfMovement.TypeOfMovementId,
                NameOfMovement = typeOfMovement.NameOfMovement,
                DescriptionOfMovement = typeOfMovement.DescriptionOfMovement
            };

            return CreatedAtAction("GetTypeOfMovement", new { id = typeOfMovement.TypeOfMovementId }, typeOfMovementDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTypeOfMovement(int id, [FromBody] UpdateTypeOfMovementDTO updateTypeOfMovementDTO)
        {
            // Verificar si el TypeOfMovement existe y no está marcado como eliminado
            var typeOfMovement = await _context.TypeOfMovements.FindAsync(id);

            if (typeOfMovement == null || typeOfMovement.IsDeleted == "1")
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

            // Actualizar el TypeOfMovement
            typeOfMovement.NameOfMovement = updateTypeOfMovementDTO.NameOfMovement;
            typeOfMovement.DescriptionOfMovement = updateTypeOfMovementDTO.DescriptionOfMovement;
            typeOfMovement.UpdatedAt = DateTime.UtcNow;
            typeOfMovement.UpdatedBy = userId;  // Asignar el ID del usuario desde el JWT

            _context.Entry(typeOfMovement).State = EntityState.Modified;

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

        // DELETE: api/TypeOfMovements/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTypeOfMovement(int id)
        {
            var typeOfMovement = await _context.TypeOfMovements.FindAsync(id);
            if (typeOfMovement == null)
            {
                return NotFound();
            }

            // Implementar eliminación lógica
            typeOfMovement.IsDeleted = "1";
            typeOfMovement.UpdatedAt = DateTime.UtcNow;

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

        private bool TypeOfMovementExists(int id)
        {
            return _context.TypeOfMovements.Any(e => e.TypeOfMovementId == id && e.IsDeleted == "0");
        }
    }
}
