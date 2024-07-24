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
    public class HandlingUnitsController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public HandlingUnitsController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/HandlingUnits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HandlingUnit>>> GetHandlingUnits()
        {
            return await _context.HandlingUnits.ToListAsync();
        }

        // GET: api/HandlingUnits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HandlingUnit>> GetHandlingUnit(int id)
        {
            var handlingUnit = await _context.HandlingUnits.FindAsync(id);

            if (handlingUnit == null)
            {
                return NotFound();
            }

            return handlingUnit;
        }

        // PUT: api/HandlingUnits/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHandlingUnit(int id, HandlingUnit handlingUnit)
        {
            if (id != handlingUnit.HandlingUnitId)
            {
                return BadRequest();
            }

            _context.Entry(handlingUnit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HandlingUnitExists(id))
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

        // POST: api/HandlingUnits
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<HandlingUnit>> PostHandlingUnit(HandlingUnit handlingUnit)
        {
            _context.HandlingUnits.Add(handlingUnit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHandlingUnit", new { id = handlingUnit.HandlingUnitId }, handlingUnit);
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

            _context.HandlingUnits.Remove(handlingUnit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HandlingUnitExists(int id)
        {
            return _context.HandlingUnits.Any(e => e.HandlingUnitId == id);
        }
    }
}
