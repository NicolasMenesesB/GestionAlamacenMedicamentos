using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs.HandlingUnitDTOs;
using System.Data.SqlTypes;
using System.Security.Claims;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HandlingUnitsController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public HandlingUnitsController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/HandlingUnits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HandlingUnitDTO>>> GetHandlingUnits()
        {
            return await _context.HandlingUnits
                 .Where(hu => hu.IsDeleted == "0")
                 .Select(hu => new HandlingUnitDTO
                 {
                     HandlingUnitId = hu.HandlingUnitId,
                     NameUnit = hu.NameUnit
                 })
                 .ToListAsync();
        }

        // GET: api/HandlingUnits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HandlingUnitDTO>> GetHandlingUnit(int id)
        {
            var handlingUnit = await _context.HandlingUnits.FindAsync(id);

            if (handlingUnit == null || handlingUnit.IsDeleted == "1")
            {
                return NotFound();
            }

            var handlingUnitDTO = new HandlingUnitDTO
            {
                HandlingUnitId = handlingUnit.HandlingUnitId,
                NameUnit = handlingUnit.NameUnit
            };

            return handlingUnitDTO;
        }

        [HttpPost]
        public async Task<ActionResult<HandlingUnitDTO>> PostHandlingUnit([FromBody] CreateHandlingUnitDTO createHandlingUnitDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtén el ID del usuario desde el token
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var handlingUnit = new HandlingUnit
            {
                NameUnit = createHandlingUnitDTO.NameUnit,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,  // Usa el ID del usuario autenticado
                IsDeleted = "0"
            };

            _context.HandlingUnits.Add(handlingUnit);
            await _context.SaveChangesAsync();

            var handlingUnitDTO = new HandlingUnitDTO
            {
                HandlingUnitId = handlingUnit.HandlingUnitId,
                NameUnit = handlingUnit.NameUnit
            };

            return CreatedAtAction("GetHandlingUnit", new { id = handlingUnit.HandlingUnitId }, handlingUnitDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutHandlingUnit(int id, [FromBody] UpdateHandlingUnitDTO updateHandlingUnitDTO)
        {
            var handlingUnit = await _context.HandlingUnits.FindAsync(id);

            if (handlingUnit == null || handlingUnit.IsDeleted == "1")
            {
                return NotFound();
            }

            // Obtén el ID del usuario desde el token
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            handlingUnit.NameUnit = updateHandlingUnitDTO.NameUnit;
            handlingUnit.UpdatedAt = DateTime.UtcNow;
            handlingUnit.UpdatedBy = userId;  // Usa el ID del usuario autenticado

            _context.Entry(handlingUnit).State = EntityState.Modified;

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


        // DELETE: api/HandlingUnits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHandlingUnit(int id)
        {
            var handlingUnit = await _context.HandlingUnits.FindAsync(id);
            if (handlingUnit == null)
            {
                return NotFound();
            }

            // Implementar eliminación lógica
            handlingUnit.IsDeleted = "1";
            handlingUnit.UpdatedAt = DateTime.UtcNow;

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

        private bool HandlingUnitExists(int id)
        {
            return _context.HandlingUnits.Any(e => e.HandlingUnitId == id && e.IsDeleted == "0");
        }
    }
}
