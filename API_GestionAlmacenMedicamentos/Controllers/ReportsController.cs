using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs.ReportDTOs;
using System.Data.SqlTypes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public ReportsController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/Reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReports()
        {
            return await _context.Reports
                 .Where(r => r.IsDeleted == "0")
                 .Select(r => new ReportDTO
                 {
                     ReportId = r.ReportId,
                     ReportName = r.ReportName,
                     Description = r.Description
                 })
                 .ToListAsync();
        }

        // GET: api/Reports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportDTO>> GetReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);

            if (report == null || report.IsDeleted == "1")
            {
                return NotFound();
            }

            var reportDTO = new ReportDTO
            {
                ReportId = report.ReportId,
                ReportName = report.ReportName,
                Description = report.Description
            };

            return reportDTO;
        }

        [HttpPost]
        public async Task<ActionResult<ReportDTO>> PostReport([FromBody] CreateReportDTO createReportDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

                var report = new Report
                {
                    ReportName = createReportDTO.ReportName,
                    Description = createReportDTO.Description,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsDeleted = "0"
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                var reportDTO = new ReportDTO
                {
                    ReportId = report.ReportId,
                    ReportName = report.ReportName,
                    Description = report.Description
                };

                return CreatedAtAction("GetReport", new { id = report.ReportId }, reportDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear el reporte: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReport(int id, [FromBody] UpdateReportDTO updateReportDTO)
        {
            try
            {
                var report = await _context.Reports.FindAsync(id);

                if (report == null || report.IsDeleted == "1")
                {
                    return NotFound();
                }

                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

                report.ReportName = updateReportDTO.ReportName;
                report.Description = updateReportDTO.Description;
                report.UpdatedAt = DateTime.UtcNow;
                report.UpdatedBy = userId;

                _context.Entry(report).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error de concurrencia al actualizar el reporte: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Error de tipo SQL al actualizar el reporte: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al actualizar el reporte: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            try
            {
                var report = await _context.Reports.FindAsync(id);
                if (report == null)
                {
                    return NotFound();
                }

                report.IsDeleted = "1";
                report.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error de concurrencia al eliminar el reporte: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Error de tipo SQL al eliminar el reporte: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar el reporte: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        private bool ReportExists(int id)
        {
            return _context.Reports.Any(e => e.ReportId == id && e.IsDeleted == "0");
        }
    }
}
