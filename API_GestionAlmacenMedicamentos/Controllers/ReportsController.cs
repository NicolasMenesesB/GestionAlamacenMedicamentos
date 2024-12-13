using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public ReportsController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // 1. Reporte de ventas por almacén
        [HttpGet("ventas-por-almacen")]
        public async Task<IActionResult> GetVentasPorAlmacen()
        {
            var result = await _context.Movements
                .Where(m => m.TypeOfMovementId == 6 && m.IsDeleted == "0") // Ventas
                .GroupBy(m => new {
                    Almacen = m.Batch.MedicationHandlingUnit.Shelf.Warehouse.NameWarehouse,
                    Medicamento = m.Batch.MedicationHandlingUnit.Medication.NameMedicine
                })
                .Select(g => new
                {
                    Almacen = g.Key.Almacen,
                    Medicamento = g.Key.Medicamento,
                    CantidadVendida = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .ToListAsync();

            return Ok(result);
        }

        // 2. Medicamentos más vendidos
        [HttpGet("medicamentos-mas-vendidos")]
        public async Task<IActionResult> GetMedicamentosMasVendidos()
        {
            var result = await _context.Movements
                .Where(m => m.TypeOfMovementId == 6 && m.IsDeleted == "0")
                .GroupBy(m => m.Batch.MedicationHandlingUnit.Medication.NameMedicine)
                .Select(g => new
                {
                    Medicamento = g.Key,
                    TotalVendidos = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(x => x.TotalVendidos)
                .Take(10) // Top 10
                .ToListAsync();

            return Ok(result);
        }

        // 3. Proveedores frecuentes
        [HttpGet("proveedores-frecuentes")]
        public async Task<IActionResult> GetProveedoresFrecuentes()
        {
            var result = await _context.Batches
                .Where(b => b.IsDeleted == "0")
                .GroupBy(b => b.Supplier.NameSupplier)
                .Select(g => new
                {
                    Proveedor = g.Key,
                    TotalLotes = g.Count() // Contamos los lotes asociados
                })
                .OrderByDescending(x => x.TotalLotes)
                .Take(10) // Top 10
                .ToListAsync();

            return Ok(result);
        }

        // 4. Medicamentos perdidos por almacén
        [HttpGet("medicamentos-perdidos")]
        public async Task<IActionResult> GetMedicamentosPerdidos()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var result = await _context.Batches
                .Where(b => b.ExpirationDate < today && b.CurrentQuantity > 0 && b.IsDeleted == "0") // Lotes vencidos con stock actual
                .GroupBy(b => new
                {
                    Almacen = b.MedicationHandlingUnit.Shelf.Warehouse.NameWarehouse,
                    Medicamento = b.MedicationHandlingUnit.Medication.NameMedicine
                })
                .Select(g => new
                {
                    Almacen = g.Key.Almacen,
                    Medicamento = g.Key.Medicamento,
                    CantidadPerdida = g.Sum(b => b.CurrentQuantity) // Suma de las cantidades actuales en lotes vencidos
                })
                .OrderByDescending(x => x.CantidadPerdida)
                .ToListAsync();

            return Ok(result);
        }

        // 5. Medicamentos próximos a vencer
        [HttpGet("medicamentos-proximos-a-vencer")]
        public async Task<IActionResult> GetMedicamentosProximosAVencer()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var result = await _context.Batches
                .Where(b => b.ExpirationDate > today && b.ExpirationDate <= today.AddDays(30) && b.CurrentQuantity > 0 && b.IsDeleted == "0")
                .Select(b => new
                {
                    Medicamento = b.MedicationHandlingUnit.Medication.NameMedicine,
                    FechaVencimiento = b.ExpirationDate,
                    CantidadRestante = b.CurrentQuantity
                })
                .OrderBy(b => b.FechaVencimiento)
                .ToListAsync();

            return Ok(result);
        }

        // 6. Medicamentos con bajo stock
        [HttpGet("medicamentos-bajo-stock")]
        public async Task<IActionResult> GetMedicamentosBajoStock()
        {
            var umbral = 10; // Define el umbral de bajo stock
            var result = await _context.Batches
                .Where(b => b.CurrentQuantity <= umbral && b.IsDeleted == "0")
                .Select(b => new
                {
                    Medicamento = b.MedicationHandlingUnit.Medication.NameMedicine,
                    CantidadActual = b.CurrentQuantity,
                    Umbral = umbral
                })
                .OrderBy(b => b.CantidadActual)
                .ToListAsync();

            return Ok(result);
        }

        // 7. Inventarios por almacén
        [HttpGet("inventarios-por-almacen")]
        public async Task<IActionResult> GetInventariosPorAlmacen()
        {
            var result = await _context.Batches
                .Where(b => b.IsDeleted == "0")
                .GroupBy(b => new
                {
                    Almacen = b.MedicationHandlingUnit.Shelf.Warehouse.NameWarehouse,
                    Medicamento = b.MedicationHandlingUnit.Medication.NameMedicine
                })
                .Select(g => new
                {
                    Almacen = g.Key.Almacen,
                    Medicamento = g.Key.Medicamento,
                    CantidadDisponible = g.Sum(b => b.CurrentQuantity)
                })
                .OrderBy(x => x.Almacen)
                .ToListAsync();

            return Ok(result);
        }

        // 8. Movimientos de inventario
        [HttpGet("movimientos-inventario")]
        public async Task<IActionResult> GetMovimientosInventario()
        {
            var result = await _context.Movements
                .Where(m => m.IsDeleted == "0")
                .Select(m => new
                {
                    TipoMovimiento = m.TypeOfMovement.NameOfMovement,
                    Medicamento = m.Batch.MedicationHandlingUnit.Medication.NameMedicine,
                    Cantidad = m.Quantity,
                    Fecha = m.DateOfMoviment,
                    Almacen = m.Batch.MedicationHandlingUnit.Shelf.Warehouse.NameWarehouse
                })
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            return Ok(result);
        }

        // 9. Medicamentos con más devoluciones
        [HttpGet("medicamentos-mas-devoluciones")]
        public async Task<IActionResult> GetMedicamentosMasDevoluciones()
        {
            var result = await _context.Movements
                .Where(m => m.TypeOfMovementId == 7 && m.IsDeleted == "0") // Devoluciones
                .GroupBy(m => m.Batch.MedicationHandlingUnit.Medication.NameMedicine)
                .Select(g => new
                {
                    Medicamento = g.Key,
                    TotalDevuelto = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(x => x.TotalDevuelto)
                .Take(10) // Top 10
                .ToListAsync();

            return Ok(result);
        }
    }
}
