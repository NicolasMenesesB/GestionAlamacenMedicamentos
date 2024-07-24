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
    public class TypeOfMovementsController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public TypeOfMovementsController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/TypeOfMovements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeOfMovement>>> GetTypeOfMovements()
        {
            return await _context.TypeOfMovements.ToListAsync();
        }

        // GET: api/TypeOfMovements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeOfMovement>> GetTypeOfMovement(int id)
        {
            var typeOfMovement = await _context.TypeOfMovements.FindAsync(id);

            if (typeOfMovement == null)
            {
                return NotFound();
            }

            return typeOfMovement;
        }

        // PUT: api/TypeOfMovements/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTypeOfMovement(int id, TypeOfMovement typeOfMovement)
        {
            if (id != typeOfMovement.TypeOfMovementId)
            {
                return BadRequest();
            }

            _context.Entry(typeOfMovement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeOfMovementExists(id))
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

        // POST: api/TypeOfMovements
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TypeOfMovement>> PostTypeOfMovement(TypeOfMovement typeOfMovement)
        {
            _context.TypeOfMovements.Add(typeOfMovement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTypeOfMovement", new { id = typeOfMovement.TypeOfMovementId }, typeOfMovement);
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

            _context.TypeOfMovements.Remove(typeOfMovement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TypeOfMovementExists(int id)
        {
            return _context.TypeOfMovements.Any(e => e.TypeOfMovementId == id);
        }
    }
}
