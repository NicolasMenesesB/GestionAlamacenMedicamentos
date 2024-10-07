using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.DTOs.MedicationHandlingUnitDTOs;
using API_GestionAlmacenMedicamentos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MedicationHandlingUnitController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public MedicationHandlingUnitController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/MedicationHandlingUnit
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicationHandlingUnitDTO>>> GetMedicationHandlingUnits()
        {
            return await _context.MedicationHandlingUnits
                .Include(m => m.DetailMedicationHandlingUnit)
                .Include(m => m.Medication)
                .Include(m => m.HandlingUnit)
                .Include(m => m.Shelf)
                .Where(m => m.IsDeleted == "0")
                .Select(unit => new MedicationHandlingUnitDTO
                {
                    MedicationHandlingUnitId = unit.MedicationHandlingUnitId,
                    Concentration = unit.Concentration,
                    MedicationName = unit.Medication.NameMedicine,
                    HandlingUnitName = unit.HandlingUnit.NameUnit,
                    ShelfName = unit.Shelf.NameShelf,
                    StorageColdChain = unit.DetailMedicationHandlingUnit != null ? unit.DetailMedicationHandlingUnit.StorageColdChain : null,
                    PhotoSensitiveStorage = unit.DetailMedicationHandlingUnit != null ? unit.DetailMedicationHandlingUnit.PhotoSensitiveStorage : null,
                    Controlled = unit.DetailMedicationHandlingUnit != null ? unit.DetailMedicationHandlingUnit.Controlled : null,
                    Oncological = unit.DetailMedicationHandlingUnit != null ? unit.DetailMedicationHandlingUnit.Oncological : null
                })
                .ToListAsync();
        }

        // GET: api/MedicationHandlingUnit/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicationHandlingUnitDTO>> GetMedicationHandlingUnit(int id)
        {
            var unit = await _context.MedicationHandlingUnits
                .Include(m => m.DetailMedicationHandlingUnit)
                .Include(m => m.Medication)
                .Include(m => m.HandlingUnit)
                .Include(m => m.Shelf)
                .FirstOrDefaultAsync(m => m.MedicationHandlingUnitId == id && m.IsDeleted == "0");

            if (unit == null)
            {
                return NotFound();
            }

            var unitDTO = new MedicationHandlingUnitDTO
            {
                MedicationHandlingUnitId = unit.MedicationHandlingUnitId,
                Concentration = unit.Concentration,
                MedicationName = unit.Medication.NameMedicine,
                HandlingUnitName = unit.HandlingUnit.NameUnit,
                ShelfName = unit.Shelf.NameShelf,
                StorageColdChain = unit.DetailMedicationHandlingUnit?.StorageColdChain,
                PhotoSensitiveStorage = unit.DetailMedicationHandlingUnit?.PhotoSensitiveStorage,
                Controlled = unit.DetailMedicationHandlingUnit?.Controlled,
                Oncological = unit.DetailMedicationHandlingUnit?.Oncological
            };

            return unitDTO;
        }

        // POST: api/MedicationHandlingUnit
        [HttpPost]
        public async Task<ActionResult<MedicationHandlingUnitDTO>> PostMedicationHandlingUnit([FromBody] CreateMedicationHandlingUnitDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Buscar los IDs basados en los nombres
                var medication = await _context.Medications.FirstOrDefaultAsync(m => m.NameMedicine == createDTO.MedicationName);
                var handlingUnit = await _context.HandlingUnits.FirstOrDefaultAsync(h => h.NameUnit == createDTO.HandlingUnitName);
                var shelf = await _context.Shelves.FirstOrDefaultAsync(s => s.NameShelf == createDTO.ShelfName);

                if (medication == null || handlingUnit == null || shelf == null)
                {
                    return BadRequest("Alguno de los valores proporcionados no es válido.");
                }

                var medicationHandlingUnit = new MedicationHandlingUnit
                {
                    Concentration = createDTO.Concentration,
                    MedicationId = medication.MedicationId,
                    HandlingUnitId = handlingUnit.HandlingUnitId,
                    ShelfId = shelf.ShelfId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1,
                    IsDeleted = "0"
                };

                _context.MedicationHandlingUnits.Add(medicationHandlingUnit);
                await _context.SaveChangesAsync();

                var detailUnit = new DetailMedicationHandlingUnit
                {
                    DetailMedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                    StorageColdChain = createDTO.StorageColdChain ?? "0",
                    PhotoSensitiveStorage = createDTO.PhotoSensitiveStorage ?? "0",
                    Controlled = createDTO.Controlled ?? "0",
                    Oncological = createDTO.Oncological ?? "0"
                };

                _context.DetailMedicationHandlingUnits.Add(detailUnit);
                await _context.SaveChangesAsync();

                var medicationHandlingUnitDTO = new MedicationHandlingUnitDTO
                {
                    MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                    Concentration = medicationHandlingUnit.Concentration,
                    MedicationName = medication.NameMedicine,
                    HandlingUnitName = handlingUnit.NameUnit,
                    ShelfName = shelf.NameShelf,
                    StorageColdChain = detailUnit.StorageColdChain,
                    PhotoSensitiveStorage = detailUnit.PhotoSensitiveStorage,
                    Controlled = detailUnit.Controlled,
                    Oncological = detailUnit.Oncological
                };

                return CreatedAtAction("GetMedicationHandlingUnit", new { id = medicationHandlingUnit.MedicationHandlingUnitId }, medicationHandlingUnitDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear la unidad de manejo de medicamentos: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // PUT: api/MedicationHandlingUnit/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedicationHandlingUnit(int id, [FromBody] UpdateMedicationHandlingUnitDTO updateDTO)
        {
            try
            {
                var medicationHandlingUnit = await _context.MedicationHandlingUnits
                    .Include(m => m.DetailMedicationHandlingUnit)
                    .FirstOrDefaultAsync(m => m.MedicationHandlingUnitId == id && m.IsDeleted == "0");

                if (medicationHandlingUnit == null)
                {
                    return NotFound();
                }

                // Buscar los IDs basados en los nombres
                var medication = await _context.Medications.FirstOrDefaultAsync(m => m.NameMedicine == updateDTO.MedicationName);
                var handlingUnit = await _context.HandlingUnits.FirstOrDefaultAsync(h => h.NameUnit == updateDTO.HandlingUnitName);
                var shelf = await _context.Shelves.FirstOrDefaultAsync(s => s.NameShelf == updateDTO.ShelfName);

                if (medication == null || handlingUnit == null || shelf == null)
                {
                    return BadRequest("Alguno de los valores proporcionados no es válido.");
                }

                // Actualiza los campos
                medicationHandlingUnit.Concentration = updateDTO.Concentration;
                medicationHandlingUnit.MedicationId = medication.MedicationId;
                medicationHandlingUnit.HandlingUnitId = handlingUnit.HandlingUnitId;
                medicationHandlingUnit.ShelfId = shelf.ShelfId;
                medicationHandlingUnit.UpdatedAt = DateTime.UtcNow;
                medicationHandlingUnit.UpdatedBy = 1;

                if (medicationHandlingUnit.DetailMedicationHandlingUnit != null)
                {
                    medicationHandlingUnit.DetailMedicationHandlingUnit.StorageColdChain = updateDTO.StorageColdChain ?? medicationHandlingUnit.DetailMedicationHandlingUnit.StorageColdChain;
                    medicationHandlingUnit.DetailMedicationHandlingUnit.PhotoSensitiveStorage = updateDTO.PhotoSensitiveStorage ?? medicationHandlingUnit.DetailMedicationHandlingUnit.PhotoSensitiveStorage;
                    medicationHandlingUnit.DetailMedicationHandlingUnit.Controlled = updateDTO.Controlled ?? medicationHandlingUnit.DetailMedicationHandlingUnit.Controlled;
                    medicationHandlingUnit.DetailMedicationHandlingUnit.Oncological = updateDTO.Oncological ?? medicationHandlingUnit.DetailMedicationHandlingUnit.Oncological;
                }

                _context.Entry(medicationHandlingUnit).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno: {ex.Message}");
            }
        }

        // DELETE: api/MedicationHandlingUnit/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicationHandlingUnit(int id)
        {
            try
            {
                var medicationHandlingUnit = await _context.MedicationHandlingUnits
                    .Include(m => m.DetailMedicationHandlingUnit)
                    .FirstOrDefaultAsync(m => m.MedicationHandlingUnitId == id && m.IsDeleted == "0");

                if (medicationHandlingUnit == null)
                {
                    return NotFound();
                }

                medicationHandlingUnit.IsDeleted = "1";
                medicationHandlingUnit.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar la unidad: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        private bool MedicationHandlingUnitExists(int id)
        {
            return _context.MedicationHandlingUnits.Any(e => e.MedicationHandlingUnitId == id && e.IsDeleted == "0");
        }
    }
}
