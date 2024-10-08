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
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public BatchesController(DbGestionAlmacenMedicamentosContext context)
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
                    MedicationHandlingUnitName = batch.MedicationHandlingUnit.HandlingUnit.NameUnit, // Nombre de la unidad de manejo desde HandlingUnit
                    SupplierName = batch.Supplier.NameSupplier // Nombre del proveedor desde Supplier
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
                MedicationHandlingUnitName = batch.MedicationHandlingUnit.HandlingUnit.NameUnit,
                SupplierName = batch.Supplier.NameSupplier
            };

            return batchDTO;
        }

        // POST: api/Batches
        [HttpPost]
        public async Task<ActionResult<BatchDTO>> PostBatch([FromBody] CreateBatchDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Buscar los IDs basados en los nombres
                var medicationHandlingUnit = await _context.MedicationHandlingUnits
                    .Include(mhu => mhu.HandlingUnit)
                    .FirstOrDefaultAsync(mhu => mhu.HandlingUnit.NameUnit == createDTO.MedicationHandlingUnitName);
                var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.NameSupplier == createDTO.SupplierName);

                if (medicationHandlingUnit == null || supplier == null)
                {
                    return BadRequest("Alguno de los valores proporcionados no es válido.");
                }

                var batch = new Batch
                {
                    BatchCode = createDTO.BatchCode,
                    FabricationDate = DateOnly.Parse(createDTO.FabricationDate),
                    ExpirationDate = DateOnly.Parse(createDTO.ExpirationDate),
                    InitialQuantity = createDTO.InitialQuantity,
                    CurrentQuantity = createDTO.CurrentQuantity,
                    MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                    SupplierId = supplier.SupplierId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1, // Ajustar según lógica de negocio
                    IsDeleted = "0"
                };

                _context.Batches.Add(batch);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetBatch", new { id = batch.BatchId }, batch);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear el lote: {ex.Message}");
            }
        }

        // PUT: api/Batches/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBatch(int id, [FromBody] UpdateBatchDTO updateDTO)
        {
            try
            {
                var batch = await _context.Batches.FindAsync(id);

                if (batch == null || batch.IsDeleted == "1")
                {
                    return NotFound();
                }

                // Buscar los IDs basados en los nombres
                var medicationHandlingUnit = await _context.MedicationHandlingUnits
                    .Include(mhu => mhu.HandlingUnit)
                    .FirstOrDefaultAsync(mhu => mhu.HandlingUnit.NameUnit == updateDTO.MedicationHandlingUnitName);
                var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.NameSupplier == updateDTO.SupplierName);

                if (medicationHandlingUnit == null || supplier == null)
                {
                    return BadRequest("Alguno de los valores proporcionados no es válido.");
                }

                // Actualizar propiedades
                batch.BatchCode = updateDTO.BatchCode;
                batch.FabricationDate = DateOnly.Parse(updateDTO.FabricationDate);
                batch.ExpirationDate = DateOnly.Parse(updateDTO.ExpirationDate);
                batch.InitialQuantity = updateDTO.InitialQuantity;
                batch.CurrentQuantity = updateDTO.CurrentQuantity;
                batch.MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId;
                batch.SupplierId = supplier.SupplierId;
                batch.UpdatedAt = DateTime.UtcNow;
                batch.UpdatedBy = 1; // Ajustar según lógica de negocio

                _context.Entry(batch).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno: {ex.Message}");
            }
        }

        // DELETE: api/Batches/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBatch(int id)
        {
            try
            {
                var batch = await _context.Batches.FindAsync(id);

                if (batch == null || batch.IsDeleted == "1")
                {
                    return NotFound();
                }

                batch.IsDeleted = "1";
                batch.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar el lote: {ex.Message}");
            }
        }

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

                    // Crear registro en DetailMedicationHandlingUnit
                    var detailMedicationHandlingUnit = new DetailMedicationHandlingUnit
                    {
                        DetailMedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId, // Relación 1 a 1
                        StorageColdChain = createDTO.StorageColdChain ?? "0",
                        PhotoSensitiveStorage = createDTO.PhotoSensitiveStorage ?? "0",
                        Controlled = createDTO.Controlled ?? "0",
                        Oncological = createDTO.Oncological ?? "0",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.DetailMedicationHandlingUnits.Add(detailMedicationHandlingUnit);
                    await _context.SaveChangesAsync();

                    // Crear batch
                    var batch = new Batch
                    {
                        BatchCode = createDTO.BatchCode,
                        FabricationDate = DateOnly.Parse(createDTO.FabricationDate),
                        ExpirationDate = DateOnly.Parse(createDTO.ExpirationDate),
                        InitialQuantity = createDTO.InitialQuantity,
                        CurrentQuantity = createDTO.CurrentQuantity,
                        MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                        SupplierId = createDTO.SupplierId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.Batches.Add(batch);
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

                    // Crear registro en DetailMedicationHandlingUnit
                    var detailMedicationHandlingUnit = new DetailMedicationHandlingUnit
                    {
                        DetailMedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId, // Relación 1 a 1
                        StorageColdChain = createDTO.StorageColdChain ?? "0",
                        PhotoSensitiveStorage = createDTO.PhotoSensitiveStorage ?? "0",
                        Controlled = createDTO.Controlled ?? "0",
                        Oncological = createDTO.Oncological ?? "0",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.DetailMedicationHandlingUnits.Add(detailMedicationHandlingUnit);
                    await _context.SaveChangesAsync();

                    // Crear batch
                    var batch = new Batch
                    {
                        BatchCode = createDTO.BatchCode,
                        FabricationDate = DateOnly.Parse(createDTO.FabricationDate),
                        ExpirationDate = DateOnly.Parse(createDTO.ExpirationDate),
                        InitialQuantity = createDTO.InitialQuantity,
                        CurrentQuantity = createDTO.CurrentQuantity,
                        MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                        SupplierId = createDTO.SupplierId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1, // Ajustar según lógica de negocio
                        IsDeleted = "0"
                    };
                    _context.Batches.Add(batch);
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

        [HttpGet("checkBatchCode/{batchCode}")]
        public async Task<IActionResult> CheckBatchCodeExists(string batchCode)
        {
            bool exists = await _context.Batches.AnyAsync(b => b.BatchCode == batchCode && b.IsDeleted == "0");
            return Ok(new { exists });
        }


        private bool BatchExists(int id)
        {
            return _context.Batches.Any(e => e.BatchId == id && e.IsDeleted == "0");
        }
    }
}
