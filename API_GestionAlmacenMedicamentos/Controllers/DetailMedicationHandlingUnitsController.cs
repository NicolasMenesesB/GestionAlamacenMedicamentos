using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.DTOs.DetailMedicationHandlingUnitDTOs;
using API_GestionAlmacenMedicamentos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetailMedicationHandlingUnitsController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public DetailMedicationHandlingUnitsController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/DetailMedicationHandlingUnits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetailMedicationHandlingUnitDTO>>> GetDetailMedicationHandlingUnits()
        {
            return await _context.DetailMedicationHandlingUnits
                .Where(d => d.IsDeleted == "0")
                .Select(d => new DetailMedicationHandlingUnitDTO
                {
                    DetailMedicationHandlingUnitId = d.DetailMedicationHandlingUnitId,
                    StorageColdChain = d.StorageColdChain,
                    PhotoSensitiveStorage = d.PhotoSensitiveStorage,
                    Controlled = d.Controlled,
                    Oncological = d.Oncological
                })
                .ToListAsync();
        }

        // POST: api/DetailMedicationHandlingUnits
        [HttpPost]
        public async Task<ActionResult<DetailMedicationHandlingUnitDTO>> PostDetailMedicationHandlingUnit(CreateDetailMedicationHandlingUnitDTO createDTO)
        {
            var detailMedicationHandlingUnit = new DetailMedicationHandlingUnit
            {
                StorageColdChain = createDTO.StorageColdChain,
                PhotoSensitiveStorage = createDTO.PhotoSensitiveStorage,
                Controlled = createDTO.Controlled,
                Oncological = createDTO.Oncological,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1,
                IsDeleted = "0"
            };

            _context.DetailMedicationHandlingUnits.Add(detailMedicationHandlingUnit);
            await _context.SaveChangesAsync();

            var detailMedicationHandlingUnitDTO = new DetailMedicationHandlingUnitDTO
            {
                DetailMedicationHandlingUnitId = detailMedicationHandlingUnit.DetailMedicationHandlingUnitId,
                StorageColdChain = detailMedicationHandlingUnit.StorageColdChain,
                PhotoSensitiveStorage = detailMedicationHandlingUnit.PhotoSensitiveStorage,
                Controlled = detailMedicationHandlingUnit.Controlled,
                Oncological = detailMedicationHandlingUnit.Oncological
            };

            return CreatedAtAction(nameof(GetDetailMedicationHandlingUnits), new { id = detailMedicationHandlingUnit.DetailMedicationHandlingUnitId }, detailMedicationHandlingUnitDTO);
        }

        // PUT: api/DetailMedicationHandlingUnits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetailMedicationHandlingUnit(int id, UpdateDetailMedicationHandlingUnitDTO updateDTO)
        {
            var detailMedicationHandlingUnit = await _context.DetailMedicationHandlingUnits.FindAsync(id);

            if (detailMedicationHandlingUnit == null || detailMedicationHandlingUnit.IsDeleted == "1")
            {
                return NotFound();
            }

            detailMedicationHandlingUnit.StorageColdChain = updateDTO.StorageColdChain;
            detailMedicationHandlingUnit.PhotoSensitiveStorage = updateDTO.PhotoSensitiveStorage;
            detailMedicationHandlingUnit.Controlled = updateDTO.Controlled;
            detailMedicationHandlingUnit.Oncological = updateDTO.Oncological;
            detailMedicationHandlingUnit.UpdatedAt = DateTime.UtcNow;
            detailMedicationHandlingUnit.UpdatedBy = 1;

            _context.Entry(detailMedicationHandlingUnit).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/DetailMedicationHandlingUnits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetailMedicationHandlingUnit(int id)
        {
            var detailMedicationHandlingUnit = await _context.DetailMedicationHandlingUnits.FindAsync(id);
            if (detailMedicationHandlingUnit == null)
            {
                return NotFound();
            }

            detailMedicationHandlingUnit.IsDeleted = "1";
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
