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
                var currentWarehouseId = GetCurrentWarehouseId();

                var batchDTO = await _context.Batches
                    .Where(b => b.BatchId == id && b.IsDeleted == "0" &&
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
                        MedicationHandlingUnitName = batch.MedicationHandlingUnit.HandlingUnit.NameUnit ?? "N/A",
                        SupplierName = batch.Supplier.NameSupplier ?? "N/A"
                    })
                    .FirstOrDefaultAsync();

                if (batchDTO == null)
                {
                    return NotFound();
                }

                return Ok(batchDTO);
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
                // Buscar el lote junto con su unidad de manejo y estante relacionado
                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit) // Incluir la unidad de manejo
                        .ThenInclude(mhu => mhu.Shelf) // Incluir el estante
                    .FirstOrDefaultAsync(b => b.BatchId == id && b.IsDeleted == "0");

                if (batch == null)
                {
                    return NotFound(new { success = false, message = "Lote no encontrado." });
                }

                // Verificar permisos del usuario
                if (GetCurrentUserRole() != "0" && batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Acceso denegado: no tiene permisos para este almacén." });
                }

                // Eliminar lógicamente todos los movimientos asociados al lote
                var movements = await _context.Movements
                    .Where(m => m.BatchId == batch.BatchId && m.IsDeleted == "0")
                    .ToListAsync();

                foreach (var movement in movements)
                {
                    movement.IsDeleted = "1";
                    movement.UpdatedAt = DateTime.UtcNow;
                }

                // Marcar el lote como eliminado lógicamente
                batch.IsDeleted = "1";
                batch.UpdatedAt = DateTime.UtcNow;

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error al eliminar el lote: {ex.Message}" });
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

        // GET: api/Warehouses/current
        [HttpGet("currentWarehouse")]
        public async Task<ActionResult> GetCurrentWarehouse()
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (currentUserRole != "0") // Si no es administrador
            {
                var currentWarehouseId = User.Claims.FirstOrDefault(c => c.Type == "WarehouseId")?.Value;
                if (string.IsNullOrEmpty(currentWarehouseId))
                {
                    return NotFound("No se encontró el almacén para este usuario.");
                }

                var warehouse = await _context.Warehouses
                    .Where(w => w.WarehouseId == int.Parse(currentWarehouseId) && w.IsDeleted == "0")
                    .Select(w => new { w.WarehouseId, w.NameWarehouse })
                    .FirstOrDefaultAsync();

                if (warehouse == null)
                {
                    return NotFound("El almacén no existe o está eliminado.");
                }

                return Ok(warehouse);
            }

            // Si es administrador, retorna todos los almacenes
            var warehouses = await _context.Warehouses
                .Where(w => w.IsDeleted == "0")
                .Select(w => new { w.WarehouseId, w.NameWarehouse })
                .ToListAsync();

            return Ok(warehouses);
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
