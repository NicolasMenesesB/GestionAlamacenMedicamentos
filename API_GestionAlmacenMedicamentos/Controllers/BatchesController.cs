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

        // GET: api/Batches
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BatchDTO>>> GetBatches()
        {
            return await _context.Batches
                .Include(b => b.MedicationHandlingUnit)
                    .ThenInclude(mhu => mhu.HandlingUnit) // Incluir la tabla HandlingUnit a través de MedicationHandlingUnit
                .Include(b => b.Supplier)
                .Where(b => b.IsDeleted == "0")
                .Select(batch => new BatchDTO
                {
                    BatchId = batch.BatchId,
                    BatchCode = batch.BatchCode,
                    FabricationDate = batch.FabricationDate.ToString("yyyy-MM-dd"),
                    ExpirationDate = batch.ExpirationDate.ToString("yyyy-MM-dd"),
                    InitialQuantity = batch.InitialQuantity,
                    CurrentQuantity = batch.CurrentQuantity,
                    MinimumStock = batch.MinimumStock, // Campo agregado para el stock mínimo
                    MedicationHandlingUnitName = batch.MedicationHandlingUnit.HandlingUnit.NameUnit, // Nombre de la unidad de manejo
                    SupplierName = batch.Supplier.NameSupplier // Nombre del proveedor
                })
                .ToListAsync();
        }

        // GET: api/Batches/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BatchDTO>> GetBatch(int id)
        {
            var batch = await _context.Batches
                .Include(b => b.MedicationHandlingUnit)
                    .ThenInclude(mhu => mhu.HandlingUnit) // Incluir la tabla HandlingUnit a través de MedicationHandlingUnit
                .Include(b => b.Supplier)
                .FirstOrDefaultAsync(b => b.BatchId == id && b.IsDeleted == "0");

            if (batch == null)
            {
                return NotFound();
            }

            var batchDTO = new BatchDTO
            {
                BatchId = batch.BatchId,
                BatchCode = batch.BatchCode,
                FabricationDate = batch.FabricationDate.ToString("yyyy-MM-dd"),
                ExpirationDate = batch.ExpirationDate.ToString("yyyy-MM-dd"),
                InitialQuantity = batch.InitialQuantity,
                CurrentQuantity = batch.CurrentQuantity,
                MinimumStock = batch.MinimumStock, // Campo agregado para el stock mínimo
                MedicationHandlingUnitName = batch.MedicationHandlingUnit.HandlingUnit.NameUnit, // Nombre de la unidad de manejo
                SupplierName = batch.Supplier.NameSupplier // Nombre del proveedor
            };

            return batchDTO;
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
                    // Crear un nuevo medicamento
                    var medication = new Medication
                    {
                        NameMedicine = createDTO.MedicationName,
                        Description = createDTO.MedicationDescription,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.Medications.Add(medication);
                    await _context.SaveChangesAsync();

                    // Crear registro en MedicationHandlingUnit
                    var medicationHandlingUnit = new MedicationHandlingUnit
                    {
                        MedicationId = medication.MedicationId,
                        HandlingUnitId = createDTO.HandlingUnitId,
                        ShelfId = createDTO.ShelfId,
                        Concentration = createDTO.Concentration,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.MedicationHandlingUnits.Add(medicationHandlingUnit);
                    await _context.SaveChangesAsync();

                    // Crear batch
                    var batch = new Batch
                    {
                        BatchCode = createDTO.BatchCode,
                        FabricationDate = DateOnly.Parse(createDTO.FabricationDate),
                        ExpirationDate = DateOnly.Parse(createDTO.ExpirationDate),
                        InitialQuantity = createDTO.InitialQuantity,
                        CurrentQuantity = createDTO.CurrentQuantity,
                        MinimumStock = createDTO.MinimumStock, // Stock mínimo añadido aquí
                        MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                        SupplierId = createDTO.SupplierId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.Batches.Add(batch);
                    await _context.SaveChangesAsync();

                    // Crear movimiento relacionado con el lote
                    var movement = new Movement
                    {
                        Quantity = createDTO.Quantity,
                        DateOfMoviment = DateOnly.Parse(createDTO.FabricationDate),
                        TypeOfMovementId = (await _context.TypeOfMovements.FirstOrDefaultAsync(t => t.NameOfMovement == createDTO.NameOfMovement)).TypeOfMovementId,
                        BatchId = batch.BatchId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.Movements.Add(movement);
                    await _context.SaveChangesAsync();

                    // Commit de la transacción
                    await transaction.CommitAsync();

                    return CreatedAtAction("GetBatch", new { id = batch.BatchId }, batch);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear el lote: {ex.Message}");
                }
            }
        }

        // POST: api/Batches/partial
        [HttpPost("partial")]
        public async Task<ActionResult> CreatePartialBatch([FromBody] CreatePartialBatchDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Buscar el medicamento existente
                    var medication = await _context.Medications.FindAsync(createDTO.MedicationId);

                    if (medication == null || medication.IsDeleted == "1")
                    {
                        return BadRequest("El medicamento no existe o ha sido eliminado.");
                    }

                    // Crear registro en MedicationHandlingUnit
                    var medicationHandlingUnit = new MedicationHandlingUnit
                    {
                        MedicationId = medication.MedicationId,
                        HandlingUnitId = createDTO.HandlingUnitId,
                        ShelfId = createDTO.ShelfId,
                        Concentration = createDTO.Concentration,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.MedicationHandlingUnits.Add(medicationHandlingUnit);
                    await _context.SaveChangesAsync();

                    // Crear batch
                    var batch = new Batch
                    {
                        BatchCode = createDTO.BatchCode,
                        FabricationDate = DateOnly.Parse(createDTO.FabricationDate),
                        ExpirationDate = DateOnly.Parse(createDTO.ExpirationDate),
                        InitialQuantity = createDTO.InitialQuantity,
                        CurrentQuantity = createDTO.CurrentQuantity,
                        MinimumStock = createDTO.MinimumStock, // Stock mínimo añadido aquí
                        MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                        SupplierId = createDTO.SupplierId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.Batches.Add(batch);
                    await _context.SaveChangesAsync();

                    // Crear movimiento relacionado con el lote
                    var movement = new Movement
                    {
                        Quantity = createDTO.Quantity,
                        DateOfMoviment = DateOnly.Parse(createDTO.FabricationDate),
                        TypeOfMovementId = (await _context.TypeOfMovements.FirstOrDefaultAsync(t => t.NameOfMovement == createDTO.NameOfMovement)).TypeOfMovementId,
                        BatchId = batch.BatchId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.Movements.Add(movement);
                    await _context.SaveChangesAsync();

                    // Commit de la transacción
                    await transaction.CommitAsync();

                    return CreatedAtAction("GetBatch", new { id = batch.BatchId }, batch);
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
            var batch = await _context.Batches.FindAsync(id);
            if (batch == null)
            {
                return NotFound();
            }

            batch.IsDeleted = "1"; // Marcar como eliminado
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Batches/expiringSoon
        [HttpGet("expiringSoon")]
        public async Task<ActionResult<IEnumerable<BatchDTO>>> GetBatchesExpiringSoon()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var nextMonth = today.AddMonths(1);

            var expiringSoonBatches = await _context.Batches
                .Where(b => b.IsDeleted == "0" && b.ExpirationDate <= nextMonth && b.ExpirationDate > today)
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
