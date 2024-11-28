using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AlertsController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;

        public AlertsController(Data.DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        private int? GetCurrentWarehouseId()
        {
            var warehouseId = User.Claims.FirstOrDefault(c => c.Type == "WarehouseId")?.Value;
            return string.IsNullOrEmpty(warehouseId) ? null : int.Parse(warehouseId);
        }

        private string GetCurrentUserRole()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // GET: api/Alerts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Alert>>> GetAlerts()
        {
            try
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                var userRole = GetCurrentUserRole();

                // Si no es administrador y no tiene WarehouseId asignado
                if (userRole != "0" && currentWarehouseId == null)
                {
                    return Forbid("Acceso denegado: no se puede determinar el almacén del usuario.");
                }

                var alerts = await _context.Alerts
                    .Include(a => a.Batch)
                        .ThenInclude(b => b.MedicationHandlingUnit)
                            .ThenInclude(mhu => mhu.Shelf)
                    .Where(a => userRole == "0" || a.Batch.MedicationHandlingUnit.Shelf.WarehouseId == currentWarehouseId)
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener las alertas: {ex.Message}");
            }
        }

        // GET: api/Alerts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Alert>> GetAlert(int id)
        {
            try
            {
                var alert = await _context.Alerts
                    .Include(a => a.Batch)
                        .ThenInclude(b => b.MedicationHandlingUnit)
                            .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(a => a.AlertId == id);

                if (alert == null)
                {
                    return NotFound("Alerta no encontrada.");
                }

                var currentWarehouseId = GetCurrentWarehouseId();
                var userRole = GetCurrentUserRole();

                if (userRole != "0" && alert.Batch.MedicationHandlingUnit.Shelf.WarehouseId != currentWarehouseId)
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                return Ok(alert);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener la alerta: {ex.Message}");
            }
        }

        // PUT: api/Alerts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlert(int id, Alert alert)
        {
            if (id != alert.AlertId)
            {
                return BadRequest("El ID de la alerta no coincide.");
            }

            try
            {
                var existingAlert = await _context.Alerts
                    .Include(a => a.Batch)
                        .ThenInclude(b => b.MedicationHandlingUnit)
                            .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(a => a.AlertId == id);

                if (existingAlert == null)
                {
                    return NotFound("Alerta no encontrada.");
                }

                var currentWarehouseId = GetCurrentWarehouseId();
                var userRole = GetCurrentUserRole();

                if (userRole != "0" && existingAlert.Batch.MedicationHandlingUnit.Shelf.WarehouseId != currentWarehouseId)
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                _context.Entry(alert).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlertExists(id))
                {
                    return NotFound("Alerta no encontrada.");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al actualizar la alerta: {ex.Message}");
            }
        }

        // POST: api/Alerts
        [HttpPost]
        public async Task<ActionResult<Alert>> PostAlert(Alert alert)
        {
            try
            {
                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(b => b.BatchId == alert.BatchId);

                if (batch == null)
                {
                    return BadRequest("Lote asociado a la alerta no encontrado.");
                }

                var currentWarehouseId = GetCurrentWarehouseId();
                var userRole = GetCurrentUserRole();

                if (userRole != "0" && batch.MedicationHandlingUnit.Shelf.WarehouseId != currentWarehouseId)
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAlert), new { id = alert.AlertId }, alert);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear la alerta: {ex.Message}");
            }
        }

        // DELETE: api/Alerts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            try
            {
                var alert = await _context.Alerts
                    .Include(a => a.Batch)
                        .ThenInclude(b => b.MedicationHandlingUnit)
                            .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(a => a.AlertId == id);

                if (alert == null)
                {
                    return NotFound("Alerta no encontrada.");
                }

                var currentWarehouseId = GetCurrentWarehouseId();
                var userRole = GetCurrentUserRole();

                if (userRole != "0" && alert.Batch.MedicationHandlingUnit.Shelf.WarehouseId != currentWarehouseId)
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al eliminar la alerta: {ex.Message}");
            }
        }

        private bool AlertExists(int id)
        {
            return _context.Alerts.Any(e => e.AlertId == id);
        }
    }
}
