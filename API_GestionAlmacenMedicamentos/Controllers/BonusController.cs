using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.DTOs.Bonus;
using API_GestionAlmacenMedicamentos.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BonusesController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;

        public BonusesController(Data.DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        #region Métodos Auxiliares

        private int GetCurrentUserId()
        {
            return int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetCurrentUserRole()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        }

        #endregion

        #region Métodos GET

        // GET: api/Bonuses/{id}
        // Obtiene una bonificación específica por su ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<BonusDTO>> GetBonus(int id)
        {
            try
            {
                var bonus = await _context.Bonuses
                    .Where(b => b.BonusesId == id && b.IsDeleted == "0")
                    .Select(b => new BonusDTO
                    {
                        BonusesId = b.BonusesId,
                        BatchId = b.BatchId,
                        BonusAmount = b.BonusAmount,
                        BonusPrice = b.BonusPrice
                    })
                    .FirstOrDefaultAsync();

                if (bonus == null)
                {
                    return NotFound(new { success = false, message = "Bonificación no encontrada." });
                }

                return Ok(bonus);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener la bonificación: {ex.Message}");
            }
        }

        // GET: api/Bonuses
        // Obtiene todas las bonificaciones activas.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BonusDTO>>> GetBonuses()
        {
            try
            {
                var bonuses = await _context.Bonuses
                    .Where(b => b.IsDeleted == "0")
                    .Select(b => new BonusDTO
                    {
                        BonusesId = b.BonusesId,
                        BatchId = b.BatchId,
                        BonusAmount = b.BonusAmount,
                        BonusPrice = b.BonusPrice
                    })
                    .ToListAsync();

                return Ok(bonuses);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener las bonificaciones: {ex.Message}");
            }
        }

        // GET: api/Bonuses/batch/{batchId}
        // Obtiene todas las bonificaciones relacionadas con un lote específico.
        [HttpGet("batch/{batchId}")]
        public async Task<ActionResult<IEnumerable<BonusDTO>>> GetBonusesByBatch(int batchId)
        {
            try
            {
                var bonuses = await _context.Bonuses
                    .Where(b => b.BatchId == batchId && b.IsDeleted == "0")
                    .Select(b => new BonusDTO
                    {
                        BonusesId = b.BonusesId,
                        BatchId = b.BatchId,
                        BonusAmount = b.BonusAmount,
                        BonusPrice = b.BonusPrice
                    })
                    .ToListAsync();

                return Ok(bonuses);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener las bonificaciones del lote: {ex.Message}");
            }
        }

        #endregion

        #region Métodos POST

        // POST: api/Bonuses
        [HttpPost]
        public async Task<IActionResult> CreateBonus([FromBody] CreateBonusDTO createBonusDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Buscar el lote basado en el código
                    var batch = await _context.Batches.FirstOrDefaultAsync(b => b.BatchCode == createBonusDTO.BatchCode && b.IsDeleted == "0");
                    if (batch == null)
                    {
                        return NotFound(new { success = false, message = "Lote no encontrado o inactivo." });
                    }

                    // Crear el nuevo registro de bono
                    var newBonus = new Bonus
                    {
                        BatchId = batch.BatchId, // Aquí usamos el ID encontrado
                        BonusAmount = createBonusDTO.BonusAmount,
                        BonusPrice = createBonusDTO.BonusPrice,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.Bonuses.Add(newBonus);

                    // Actualizar las cantidades del lote
                    batch.InitialQuantity += createBonusDTO.BonusAmount;
                    batch.CurrentQuantity += createBonusDTO.BonusAmount;
                    batch.UpdatedAt = DateTime.UtcNow;
                    batch.UpdatedBy = GetCurrentUserId();

                    _context.Batches.Update(batch);

                    // Guardar cambios
                    await _context.SaveChangesAsync();

                    // Confirmar transacción
                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(GetBonus), new { id = newBonus.BonusesId }, new { success = true, bonus = newBonus });
                }
                catch (Exception ex)
                {
                    // Revertir transacción en caso de error
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error al crear el bono: {ex.Message}" });
                }
            }
        }

        #endregion

    }
}

