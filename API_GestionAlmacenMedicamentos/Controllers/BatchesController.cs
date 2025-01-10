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

        #region Métodos Auxiliares

        // Obtiene el ID del usuario actual a partir de los claims.
        private int GetCurrentUserId()
        {
            return int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // Obtiene el rol del usuario actual a partir de los claims.
        private string GetCurrentUserRole()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // Obtiene el ID del almacén asociado al usuario actual a partir de los claims.
        private int? GetCurrentWarehouseId()
        {
            var warehouseId = User.Claims.FirstOrDefault(c => c.Type == "WarehouseId")?.Value;
            return string.IsNullOrEmpty(warehouseId) ? null : int.Parse(warehouseId);
        }

        #endregion


        #region Métodos GET

        // GET: api/Batches
        // Obtiene una lista de lotes activos con información detallada.
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
                        .ThenInclude(mhu => mhu.Medication)
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.HandlingUnit)
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .Include(b => b.Supplier)
                    .Where(b => b.IsDeleted == "0" &&
                                (b.MedicationHandlingUnit.Shelf.WarehouseId == currentWarehouseId || GetCurrentUserRole() == "0"))
                    .ToListAsync();

                var result = batches.Select(batch => new BatchDTO
                {
                    BatchId = batch.BatchId,
                    BatchCode = batch.BatchCode,
                    FabricationDate = batch.FabricationDate.ToString("yyyy-MM-dd"),
                    ExpirationDate = batch.ExpirationDate.ToString("yyyy-MM-dd"),
                    InitialQuantity = batch.InitialQuantity,
                    CurrentQuantity = batch.CurrentQuantity,
                    MinimumStock = batch.MinimumStock,
                    unitPrice = batch.unitPrice,
                    MedicationName = batch.MedicationHandlingUnit.Medication.NameMedicine,
                    Concentration = batch.MedicationHandlingUnit.Concentration,
                    UnitMeasure = batch.MedicationHandlingUnit.HandlingUnit.NameUnit,
                    ShelfName = batch.MedicationHandlingUnit.Shelf?.NameShelf ?? "N/A",
                    WarehouseName = _context.Warehouses
                        .FirstOrDefault(w => w.WarehouseId == batch.MedicationHandlingUnit.Shelf.WarehouseId)?.NameWarehouse ?? "N/A",
                    SupplierName = batch.Supplier?.NameSupplier ?? "N/A",
                    CreatedAt = batch.CreatedAt,
                    UpdatedAt = batch.UpdatedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener los lotes: {ex.Message}");
            }
        }

        // GET: api/Batches/5
        // Obtiene los detalles de un lote específico basado en su ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<BatchDTO>> GetBatch(int id)
        {
            try
            {
                var currentWarehouseId = GetCurrentWarehouseId();

                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Medication)
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.HandlingUnit)
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .Include(b => b.Supplier)
                    .Include(b => b.Bonuses) // Incluir los bonos asociados al lote
                    .Where(b => b.BatchId == id && b.IsDeleted == "0" &&
                                (b.MedicationHandlingUnit.Shelf.WarehouseId == currentWarehouseId || GetCurrentUserRole() == "0"))
                    .FirstOrDefaultAsync();

                if (batch == null)
                {
                    return NotFound();
                }

                string warehouseName = "N/A";
                if (batch.MedicationHandlingUnit.Shelf?.WarehouseId != null)
                {
                    var warehouse = await _context.Warehouses
                        .FirstOrDefaultAsync(w => w.WarehouseId == batch.MedicationHandlingUnit.Shelf.WarehouseId);
                    warehouseName = warehouse?.NameWarehouse ?? "N/A";
                }

                // Calcular cantidad inicial real
                var totalBonusAmount = batch.Bonuses?.Sum(b => b.BonusAmount) ?? 0;
                var realInitialQuantity = batch.InitialQuantity - totalBonusAmount;

                // Calcular costo total de los bonos
                var totalBonusCost = batch.Bonuses?.Sum(b => b.BonusAmount * b.BonusPrice) ?? 0;

                // Calcular costo total del lote
                var totalBatchCost = (realInitialQuantity * batch.unitPrice) + totalBonusCost;

                var totalQuantityWithBonus = batch.InitialQuantity + totalBonusAmount;

                var batchDTO = new BatchDTO
                {
                    BatchId = batch.BatchId,
                    BatchCode = batch.BatchCode,
                    FabricationDate = batch.FabricationDate.ToString("yyyy-MM-dd"),
                    ExpirationDate = batch.ExpirationDate.ToString("yyyy-MM-dd"),
                    InitialQuantity = batch.InitialQuantity,
                    RealInitialQuantity = realInitialQuantity, // Nueva cantidad inicial
                    CurrentQuantity = batch.CurrentQuantity,
                    MinimumStock = batch.MinimumStock,
                    unitPrice = batch.unitPrice,
                    TotalBonusCost = totalBonusCost, // Nuevo: costo total de los bonos
                    TotalBatchCost = totalBatchCost, // Nuevo: costo total del lote
                    TotalQuantityWithBonus = totalQuantityWithBonus,
                    MedicationName = batch.MedicationHandlingUnit.Medication.NameMedicine ?? "Sin Medicamento Asociado",
                    Concentration = batch.MedicationHandlingUnit.Concentration ?? "N/A",
                    UnitMeasure = batch.MedicationHandlingUnit.HandlingUnit?.NameUnit ?? "N/A",
                    ShelfName = batch.MedicationHandlingUnit.Shelf?.NameShelf ?? "N/A",
                    WarehouseName = warehouseName,
                    SupplierName = batch.Supplier?.NameSupplier ?? "N/A",
                    CreatedAt = batch.CreatedAt,
                    UpdatedAt = batch.UpdatedAt
                };

                return Ok(batchDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener el lote: {ex.Message}");
            }
        }


        // GET: api/Batches/deleted
        // Obtiene una lista de lotes que han sido eliminados.
        [HttpGet("deleted")]
        public async Task<ActionResult<IEnumerable<BatchDTO>>> GetDeletedBatches()
        {
            try
            {
                var deletedBatches = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Medication)
                    .Where(b => b.IsDeleted == "1")
                    .Select(batch => new BatchDTO
                    {
                        BatchId = batch.BatchId,
                        BatchCode = batch.BatchCode,
                        ExpirationDate = batch.ExpirationDate.ToString("yyyy-MM-dd"),
                        MedicationName = batch.MedicationHandlingUnit.Medication.NameMedicine
                    })
                    .ToListAsync();

                return Ok(deletedBatches);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener lotes eliminados: {ex.Message}");
            }
        }

        // GET: api/Batches/expiringSoon
        // Obtiene una lista de lotes que están próximos a expirar.
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

        // GET: api/Batches/searchByBatchCode/{batchCode}
        // Busca un lote basado en su código de lote.
        [HttpGet("searchByBatchCode/{batchCode}")]
        public async Task<IActionResult> GetBatchByCode(string batchCode)
        {
            var batch = await _context.Batches
                .Include(b => b.MedicationHandlingUnit)
                    .ThenInclude(mhu => mhu.Medication)
                .Include(b => b.MedicationHandlingUnit)
                    .ThenInclude(mhu => mhu.HandlingUnit)
                .Include(b => b.MedicationHandlingUnit)
                    .ThenInclude(mhu => mhu.Shelf)
                .Include(b => b.Supplier)
                .FirstOrDefaultAsync(b => b.BatchCode == batchCode && b.IsDeleted == "0");

            if (batch == null)
            {
                return NotFound(new { success = false, message = "Lote no encontrado." });
            }

            var shelf = batch.MedicationHandlingUnit.Shelf;
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.WarehouseId == shelf.WarehouseId);

            return Ok(new
            {
                batch.BatchId,
                batch.BatchCode,
                batch.InitialQuantity,
                batch.CurrentQuantity,
                batch.MinimumStock,
                batch.unitPrice,
                MedicationName = batch.MedicationHandlingUnit.Medication.NameMedicine,
                Concentration = batch.MedicationHandlingUnit.Concentration,
                UnitMeasure = batch.MedicationHandlingUnit.HandlingUnit.NameUnit,
                WarehouseName = warehouse?.NameWarehouse ?? "N/A",
                ShelfName = shelf?.NameShelf ?? "N/A",
                SupplierName = batch.Supplier?.NameSupplier ?? "N/A"
            });
        }

        // GET: api/Warehouses/current
        // Obtiene información del almacén asociado al usuario actual. 
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
        // Verifica si un código de lote ya existe.
        [HttpGet("checkBatchCode/{batchCode}")]
        public async Task<IActionResult> CheckBatchCodeExists(string batchCode)
        {
            var exists = await _context.Batches.AnyAsync(b => b.BatchCode == batchCode && b.IsDeleted == "0");
            return Ok(new { exists });
        }

        #endregion


        #region Métodos POST

        // POST: api/Batches/full
        // Crea un nuevo lote completo con todos los datos requeridos, incluyendo el medicamento, la unidad de manejo, y el movimiento asociado.
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

                    // Crear registro en Detail_Medication_HandlingUnit
                    var detail = new DetailMedicationHandlingUnit
                    {
                        DetailMedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                        StorageColdChain = createDTO.StorageColdChain,
                        PhotoSensitiveStorage = createDTO.PhotoSensitiveStorage,
                        Controlled = createDTO.Controlled,
                        Oncological = createDTO.Oncological,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.DetailMedicationHandlingUnits.Add(detail);
                    await _context.SaveChangesAsync();

                    var batch = new Batch
                    {
                        BatchCode = createDTO.BatchCode,
                        FabricationDate = DateOnly.Parse(createDTO.FabricationDate),
                        ExpirationDate = DateOnly.Parse(createDTO.ExpirationDate),
                        InitialQuantity = createDTO.InitialQuantity,
                        CurrentQuantity = createDTO.InitialQuantity,
                        MinimumStock = createDTO.MinimumStock,
                        unitPrice = createDTO.UnitPrice,
                        MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                        SupplierId = createDTO.SupplierId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.Batches.Add(batch);
                    await _context.SaveChangesAsync();

                    var typeOfMovement = await _context.TypeOfMovements
                        .FirstOrDefaultAsync(t => t.NameOfMovement == createDTO.NameOfMovement);

                    if (typeOfMovement == null)
                    {
                        throw new Exception($"No se encontró el tipo de movimiento: {createDTO.NameOfMovement}.");
                    }

                    var movement = new Movement
                    {
                        Quantity = createDTO.Quantity,
                        DateOfMoviment = DateOnly.Parse(createDTO.DateOfMoviment),
                        TypeOfMovementId = typeOfMovement.TypeOfMovementId,
                        BatchId = batch.BatchId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.Movements.Add(movement);
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

        // POST: api/Batches/partial
        // Crea un lote parcial con datos específicos, incluyendo la unidad de manejo y el movimiento asociado.
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
                    var medication = await _context.Medications.FindAsync(createDTO.MedicationId);
                    if (medication == null || medication.IsDeleted == "1")
                    {
                        return BadRequest("El medicamento proporcionado no existe.");
                    }

                    var shelf = await _context.Shelves.FindAsync(createDTO.ShelfId);
                    if (shelf == null || shelf.IsDeleted == "1")
                    {
                        return BadRequest("El estante proporcionado no existe.");
                    }

                    if (GetCurrentUserRole() != "0" && shelf.WarehouseId != GetCurrentWarehouseId())
                    {
                        return Forbid("No tiene permisos para este almacén.");
                    }

                    var medicationHandlingUnit = new MedicationHandlingUnit
                    {
                        MedicationId = createDTO.MedicationId,
                        HandlingUnitId = createDTO.HandlingUnitId,
                        ShelfId = createDTO.ShelfId,
                        Concentration = createDTO.Concentration,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.MedicationHandlingUnits.Add(medicationHandlingUnit);
                    await _context.SaveChangesAsync();

                    // Crear registro en Detail_Medication_HandlingUnit
                    var detail = new DetailMedicationHandlingUnit
                    {
                        DetailMedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                        StorageColdChain = createDTO.StorageColdChain,
                        PhotoSensitiveStorage = createDTO.PhotoSensitiveStorage,
                        Controlled = createDTO.Controlled,
                        Oncological = createDTO.Oncological,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.DetailMedicationHandlingUnits.Add(detail);
                    await _context.SaveChangesAsync();

                    var batch = new Batch
                    {
                        BatchCode = createDTO.BatchCode,
                        FabricationDate = DateOnly.Parse(createDTO.FabricationDate),
                        ExpirationDate = DateOnly.Parse(createDTO.ExpirationDate),
                        InitialQuantity = createDTO.InitialQuantity,
                        CurrentQuantity = createDTO.InitialQuantity,
                        MinimumStock = createDTO.MinimumStock,
                        unitPrice = createDTO.UnitPrice,
                        MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId,
                        SupplierId = createDTO.SupplierId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.Batches.Add(batch);
                    await _context.SaveChangesAsync();

                    var typeOfMovement = await _context.TypeOfMovements
                        .FirstOrDefaultAsync(t => t.NameOfMovement == createDTO.NameOfMovement);

                    if (typeOfMovement == null)
                    {
                        throw new Exception($"No se encontró el tipo de movimiento: {createDTO.NameOfMovement}.");
                    }

                    var movement = new Movement
                    {
                        Quantity = createDTO.Quantity,
                        DateOfMoviment = DateOnly.Parse(createDTO.DateOfMoviment),
                        TypeOfMovementId = typeOfMovement.TypeOfMovementId,
                        BatchId = batch.BatchId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    };

                    _context.Movements.Add(movement);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return CreatedAtAction(nameof(GetBatch), new { id = batch.BatchId }, batch);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear el lote parcial: {ex.Message}");
                }
            }
        }

        #endregion


        #region Métodos PUT

        // PUT: api/Batches/5
        // Actualiza un lote existente con todos los datos proporcionados.
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBatch(int id, [FromBody] UpdateBatchDTO updateBatchDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .Include(b => b.Supplier)
                    .FirstOrDefaultAsync(b => b.BatchId == id && b.IsDeleted == "0");

                if (batch == null)
                {
                    return NotFound("Lote no encontrado.");
                }

                if (GetCurrentUserRole() != "0" && batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return Forbid("Acceso denegado: no tiene permisos para este almacén.");
                }

                // Actualizar las propiedades del lote
                batch.BatchCode = updateBatchDTO.BatchCode;
                batch.FabricationDate = DateOnly.Parse(updateBatchDTO.FabricationDate);
                batch.ExpirationDate = DateOnly.Parse(updateBatchDTO.ExpirationDate);
                batch.InitialQuantity = updateBatchDTO.InitialQuantity;
                batch.CurrentQuantity = updateBatchDTO.CurrentQuantity;
                batch.MinimumStock = updateBatchDTO.MinimumStock;
                batch.unitPrice = updateBatchDTO.unitPrice;

                // Actualizar relaciones con medicamento, unidad de manejo, estante y proveedor
                var medicationHandlingUnit = await _context.MedicationHandlingUnits
                    .Include(mhu => mhu.Medication)
                    .Include(mhu => mhu.HandlingUnit)
                    .Include(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(mhu => mhu.Medication.NameMedicine == updateBatchDTO.MedicationName &&
                                                mhu.Concentration == updateBatchDTO.Concentration &&
                                                mhu.HandlingUnit.NameUnit == updateBatchDTO.UnitMeasure &&
                                                mhu.Shelf.NameShelf == updateBatchDTO.ShelfName);

                if (medicationHandlingUnit == null)
                {
                    return BadRequest("La unidad de manejo proporcionada no existe.");
                }

                batch.MedicationHandlingUnitId = medicationHandlingUnit.MedicationHandlingUnitId;

                var supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.NameSupplier == updateBatchDTO.SupplierName);

                if (supplier == null)
                {
                    return BadRequest("El proveedor proporcionado no existe.");
                }

                batch.SupplierId = supplier.SupplierId;

                // Registrar los campos de auditoría
                batch.UpdatedAt = DateTime.UtcNow;
                batch.UpdatedBy = GetCurrentUserId();

                // Guardar los cambios
                _context.Entry(batch).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al actualizar el lote: {ex.Message}");
            }
        }

        #endregion


        #region Métodos DELETE y RESTORE

        // DELETE: api/Batches/5
        // Realiza una eliminación lógica de un lote y sus movimientos asociados.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBatch(int id)
        {
            try
            {
                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(b => b.BatchId == id && b.IsDeleted == "0");

                if (batch == null)
                {
                    return NotFound(new { success = false, message = "Lote no encontrado." });
                }

                if (GetCurrentUserRole() != "0" && batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Acceso denegado: no tiene permisos para este almacén." });
                }

                var movements = await _context.Movements
                    .Where(m => m.BatchId == batch.BatchId && m.IsDeleted == "0")
                    .ToListAsync();

                foreach (var movement in movements)
                {
                    movement.IsDeleted = "1";
                    movement.UpdatedAt = DateTime.UtcNow;
                }

                batch.IsDeleted = "1";
                batch.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error al eliminar el lote: {ex.Message}" });
            }
        }

        // RESTORE: api/Batches/restore/5
        // Restaura un lote previamente eliminado junto con sus movimientos asociados.
        [HttpPost("restore/{id}")]
        public async Task<IActionResult> RestoreBatch(int id)
        {
            try
            {
                var batch = await _context.Batches
                    .Include(b => b.MedicationHandlingUnit)
                        .ThenInclude(mhu => mhu.Shelf)
                    .FirstOrDefaultAsync(b => b.BatchId == id && b.IsDeleted == "1");

                if (batch == null)
                {
                    return NotFound(new { success = false, message = "Lote no encontrado o ya está activo." });
                }

                if (GetCurrentUserRole() != "0" && batch.MedicationHandlingUnit.Shelf.WarehouseId != GetCurrentWarehouseId())
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Acceso denegado: no tiene permisos para este almacén." });
                }

                batch.IsDeleted = "0";
                batch.UpdatedAt = DateTime.UtcNow;

                var movements = await _context.Movements
                    .Where(m => m.BatchId == batch.BatchId && m.IsDeleted == "1")
                    .ToListAsync();

                foreach (var movement in movements)
                {
                    movement.IsDeleted = "0";
                    movement.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Lote y movimientos asociados restablecidos correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error al restablecer el lote: {ex.Message}" });
            }
        }

        #endregion
    }
}
