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
    public class DetailMedicationHandlingUnitsController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public DetailMedicationHandlingUnitsController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/DetailMedicationHandlingUnits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetailMedicationHandlingUnit>>> GetDetailMedicationHandlingUnits()
        {
            return await _context.DetailMedicationHandlingUnits.ToListAsync();
        }

        // GET: api/DetailMedicationHandlingUnits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DetailMedicationHandlingUnit>> GetDetailMedicationHandlingUnit(int id)
        {
            var detailMedicationHandlingUnit = await _context.DetailMedicationHandlingUnits.FindAsync(id);

            if (detailMedicationHandlingUnit == null)
            {
                return NotFound();
            }

            return detailMedicationHandlingUnit;
        }

        // PUT: api/DetailMedicationHandlingUnits/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetailMedicationHandlingUnit(int id, DetailMedicationHandlingUnit detailMedicationHandlingUnit)
        {
            if (id != detailMedicationHandlingUnit.DetailMedicationHandlingUnitId)
            {
                return BadRequest();
            }

            _context.Entry(detailMedicationHandlingUnit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DetailMedicationHandlingUnitExists(id))
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

        // POST: api/DetailMedicationHandlingUnits
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DetailMedicationHandlingUnit>> PostDetailMedicationHandlingUnit(DetailMedicationHandlingUnit detailMedicationHandlingUnit)
        {
            _context.DetailMedicationHandlingUnits.Add(detailMedicationHandlingUnit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDetailMedicationHandlingUnit", new { id = detailMedicationHandlingUnit.DetailMedicationHandlingUnitId }, detailMedicationHandlingUnit);
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

            _context.DetailMedicationHandlingUnits.Remove(detailMedicationHandlingUnit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DetailMedicationHandlingUnitExists(int id)
        {
            return _context.DetailMedicationHandlingUnits.Any(e => e.DetailMedicationHandlingUnitId == id);
        }
    }
}
