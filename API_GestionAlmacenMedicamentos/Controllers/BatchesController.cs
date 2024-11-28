using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.DTOs.BatchDTOs;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs.Batch;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BatchesController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;

        public BatchesController(Data.DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetCurrentUserRole()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private int? GetCurrentWarehouseId()
        {
            var warehouseId = User.Claims.FirstOrDefault(c => c.Type == "WarehouseId")?.Value;
            return string.IsNullOrEmpty(warehouseId) ? null : int.Parse(warehouseId);
        }

        // GET: api/Batches
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BatchDTO>>> GetBatches()
        {
            try
            {
                var currentWarehouseId = GetCurrentWarehouseId();

                if (currentWarehouseId == null && GetCurrentUserRole() != "0")
                {
                    return Forbid("Acceso denegado: no se puede determinar el almacén del usuario.");
                }

                var batches = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .Include(b => b.Supplier)
                    .Where(b => b.IsDeleted == "0" &&
                                (b.MedicationHandlingUnit.Shelf.WarehouseId == currentWarehouseId || GetCurrentUserRole() == "0"))
                    .Select(batch => new BatchDTO
                    {
                        BatchId = batch.BatchId,
                        BatchCode = batch.BatchCode,
                        FabricationDate = batch.FabricationDate.ToString("yyyy-MM-dd"),
                        ExpirationDate = batch.ExpirationDate.ToString("yyyy-MM-dd"),
                        InitialQuantity = batch.InitialQuantity,
                        CurrentQuantity = batch.CurrentQuantity,
                        MinimumStock = batch.MinimumStock,
                        MedicationHandlingUnitName = batch.MedicationHandlingUnit.HandlingUnit.NameUnit,
                        SupplierName = batch.Supplier.NameSupplier
                    })
                    .ToListAsync();

                return Ok(batches);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener los lotes: {ex.Message}");
            }
        }

        // GET: api/Batches/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BatchDTO>> GetBatch(int id)
        {
            try
            {
                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .Include(b => b.Supplier)
                    .FirstOrDefaultAsync(b => b.BatchId == id && b.IsDeleted == "0");

                if (batch == null)
                {
                    return NotFound();
                }

                if (GetCurrentUserRole() != "0" && batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                var batchDTO = new BatchDTO
                {
                    BatchId = batch.BatchId,
                    BatchCode = batch.BatchCode,
                    FabricationDate = batch.FabricationDate.ToString("yyyy-MM-dd"),
                    ExpirationDate = batch.ExpirationDate.ToString("yyyy-MM-dd"),
                    InitialQuantity = batch.InitialQuantity,
                    CurrentQuantity = batch.CurrentQuantity,
                    MinimumStock = batch.MinimumStock,
                    MedicationHandlingUnitName = batch.MedicationHandlingUnit.HandlingUnit.NameUnit,
                    SupplierName = batch.Supplier.NameSupplier
                };

                return batchDTO;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener el lote: {ex.Message}");
            }
        }

        // POST: api/Batches/full
        [HttpPost("full")]
        public async Task<ActionResult> CreateFullBatch([FromBody] CreateFullBatchDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var shelf = await _context.Shelves.FindAsync(createDTO.ShelfId);

                    if (shelf == null || shelf.IsDeleted == "1")
                    {
                        return BadRequest("El estante proporcionado no existe.");
                    }

                    if (GetCurrentUserRole() != "0" && shelf.WarehouseId != GetCurrentWarehouseId())
                    {
                        return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                    }

                    var medication = new Medication
                    {
                        NameMedicine = createDTO.MedicationName,
                        Description = createDTO.MedicationDescription,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.Medications.Add(medication);
                    await _context.SaveChangesAsync();

                    var medicationHandlingUnit = new MedicationHandlingUnit
                    {
                        MedicationId = medication.MedicationId,
                        HandlingUnitId = createDTO.HandlingUnitId,
                        ShelfId = createDTO.ShelfId,
                        Concentration = createDTO.Concentration,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.MedicationHandlingUnits.Add(medicationHandlingUnit);
                    await _context.SaveChangesAsync();

                    var batch = new Batch
                    {
                        BatchCode = createDTO.BatchCode,
                        FabricationDate = DateOnly.Parse(createDTO.FabricationDate),
                        ExpirationDate = DateOnly.Parse(createDTO.ExpirationDate),
                        InitialQuantity = createDTO.InitialQuantity,
                        CurrentQuantity = createDTO.CurrentQuantity,
                        MinimumStock = createDTO.MinimumStock,
                        MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                        SupplierId = createDTO.SupplierId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.Batches.Add(batch);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(GetBatch), new { id = batch.BatchId }, batch);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear el lote: {ex.Message}");
                }
            }
        }

        // DELETE: api/Batches/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBatch(int id)
        {
            try
            {
                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(b => b.BatchId == id);

                if (batch == null || batch.IsDeleted == "1")
                {
                    return NotFound("Lote no encontrado.");
                }

                if (GetCurrentUserRole() != "0" && batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                batch.IsDeleted = "1";
                batch.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al eliminar el lote: {ex.Message}");
            }
        }

        // GET: api/Batches/expiringSoon
        [HttpGet("expiringSoon")]
        public async Task<ActionResult<IEnumerable<BatchDTO>>> GetBatchesExpiringSoon()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var nextMonth = today.AddMonths(1);

            var expiringSoonBatches = await _context.Batches
                .Include(b => b.MedicationHandlingUnit)
                    .ThenInclude(mhu => mhu.Shelf)
                .Where(b => b.IsDeleted == "0" &&
                            b.ExpirationDate <= nextMonth &&
                            b.ExpirationDate > today &&
                            (b.MedicationHandlingUnit.Shelf.WarehouseId == GetCurrentWarehouseId() || GetCurrentUserRole() == "0"))
                .Select(batch => new BatchDTO
                {
                    BatchId = batch.BatchId,
                    BatchCode = batch.BatchCode,
                    ExpirationDate = batch.ExpirationDate.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return Ok(expiringSoonBatches);
        }

        // GET: api/Batches/checkBatchCode/{batchCode}
        [HttpGet("checkBatchCode/{batchCode}")]
        public async Task<IActionResult> CheckBatchCodeExists(string batchCode)
        {
            var exists = await _context.Batches.AnyAsync(b => b.BatchCode == batchCode && b.IsDeleted == "0");
            return Ok(new { exists });
        }

    }
}
