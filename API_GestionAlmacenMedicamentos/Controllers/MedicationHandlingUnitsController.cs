using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationHandlingUnitsController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public MedicationHandlingUnitsController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/MedicationHandlingUnits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicationHandlingUnit>>> GetMedicationHandlingUnits()
        {
            return await _context.MedicationHandlingUnits.ToListAsync();
        }

        // GET: api/MedicationHandlingUnits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicationHandlingUnit>> GetMedicationHandlingUnit(int id)
        {
            var medicationHandlingUnit = await _context.MedicationHandlingUnits.FindAsync(id);

            if (medicationHandlingUnit == null)
            {
                return NotFound();
            }

            return medicationHandlingUnit;
        }

        // PUT: api/MedicationHandlingUnits/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedicationHandlingUnit(int id, MedicationHandlingUnit medicationHandlingUnit)
        {
            if (id != medicationHandlingUnit.MedicationHandlingUnitId)
            {
                return BadRequest();
            }

            _context.Entry(medicationHandlingUnit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MedicationHandlingUnitExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MedicationHandlingUnits
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MedicationHandlingUnit>> PostMedicationHandlingUnit(MedicationHandlingUnit medicationHandlingUnit)
        {
            _context.MedicationHandlingUnits.Add(medicationHandlingUnit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMedicationHandlingUnit", new { id = medicationHandlingUnit.MedicationHandlingUnitId }, medicationHandlingUnit);
        }

        // DELETE: api/MedicationHandlingUnits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicationHandlingUnit(int id)
        {
            var medicationHandlingUnit = await _context.MedicationHandlingUnits.FindAsync(id);
            if (medicationHandlingUnit == null)
            {
                return NotFound();
            }

            _context.MedicationHandlingUnits.Remove(medicationHandlingUnit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MedicationHandlingUnitExists(int id)
        {
            return _context.MedicationHandlingUnits.Any(e => e.MedicationHandlingUnitId == id);
        }
    }
}
