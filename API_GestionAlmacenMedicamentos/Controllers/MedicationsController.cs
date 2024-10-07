using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs.MedicationDTOs;
using System.Data.SqlTypes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationsController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public MedicationsController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/Medications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicationDTO>>> GetMedications()
        {
            return await _context.Medications
                 .Where(m => m.IsDeleted == "0")
                 .Select(m => new MedicationDTO
                 {
                     MedicationId = m.MedicationId,
                     NameMedicine = m.NameMedicine,
                     Description = m.Description
                 })
                 .ToListAsync();
        }

        // GET: api/Medications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicationDTO>> GetMedication(int id)
        {
            var medication = await _context.Medications.FindAsync(id);

            if (medication == null || medication.IsDeleted == "1")
            {
                return NotFound();
            }

            var medicationDTO = new MedicationDTO
            {
                MedicationId = medication.MedicationId,
                NameMedicine = medication.NameMedicine,
                Description = medication.Description
            };

            return medicationDTO;
        }

        [HttpPost]
        public async Task<ActionResult<MedicationDTO>> PostMedication([FromBody] CreateMedicationDTO createMedicationDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

                var medication = new Medication
                {
                    NameMedicine = createMedicationDTO.NameMedicine,
                    Description = createMedicationDTO.Description,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsDeleted = "0"
                };

                _context.Medications.Add(medication);
                await _context.SaveChangesAsync();

                var medicationDTO = new MedicationDTO
                {
                    MedicationId = medication.MedicationId,
                    NameMedicine = medication.NameMedicine,
                    Description = medication.Description
                };

                return CreatedAtAction("GetMedication", new { id = medication.MedicationId }, medicationDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear el medicamento: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedication(int id, [FromBody] UpdateMedicationDTO updateMedicationDTO)
        {
            try
            {
                var medication = await _context.Medications.FindAsync(id);

                if (medication == null || medication.IsDeleted == "1")
                {
                    return NotFound();
                }

                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

                medication.NameMedicine = updateMedicationDTO.NameMedicine;
                medication.Description = updateMedicationDTO.Description;
                medication.UpdatedAt = DateTime.UtcNow;
                medication.UpdatedBy = userId;

                _context.Entry(medication).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error de concurrencia al actualizar el medicamento: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Error de tipo SQL al actualizar el medicamento: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al actualizar el medicamento: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedication(int id)
        {
            try
            {
                var medication = await _context.Medications.FindAsync(id);
                if (medication == null)
                {
                    return NotFound();
                }

                medication.IsDeleted = "1";
                medication.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error de concurrencia al eliminar el medicamento: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Error de tipo SQL al eliminar el medicamento: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar el medicamento: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        private bool MedicationExists(int id)
        {
            return _context.Medications.Any(e => e.MedicationId == id && e.IsDeleted == "0");
        }
    }
}
