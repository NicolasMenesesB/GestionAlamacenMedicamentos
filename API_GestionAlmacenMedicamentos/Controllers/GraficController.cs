using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraficoController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;

        public GraficoController(Data.DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        private string GetUserWarehouseId()
        {
            return User.FindFirst("WarehouseId")?.Value;
        }

        [HttpGet("medicamentos-mas-vendidos")]
        public async Task<IActionResult> GetMedicamentosMasVendidos()
        {
            var userRole = GetUserRole();
            var warehouseId = GetUserWarehouseId();

            var query = _context.Movements
                .Where(m => m.TypeOfMovementId == 6 && m.IsDeleted == "0"); // Salida por Venta

            if (userRole != "0" && !string.IsNullOrEmpty(warehouseId))
            {
                // Filtrar por WarehouseId para trabajadores
                query = query.Where(m => m.Batch.MedicationHandlingUnit.Shelf.Warehouse.WarehouseId.ToString() == warehouseId);
            }

            var result = await query
                .GroupBy(m => m.Batch.MedicationHandlingUnit.Medication.NameMedicine)
                .Select(g => new
                {
                    Medicamento = g.Key,
                    TotalVendido = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(x => x.TotalVendido)
                .Take(10) // Obtener los 10 más vendidos
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("medicamentos-perdidos-vencimiento")]
        public async Task<IActionResult> GetMedicamentosPerdidosPorVencimiento()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var result = await _context.Batches
                .Where(b => b.ExpirationDate < today && b.CurrentQuantity > 0 && b.IsDeleted == "0") // Lotes vencidos con stock actual
                .GroupBy(b => b.MedicationHandlingUnit.Medication.NameMedicine)
                .Select(g => new
                {
                    Medicamento = g.Key,
                    TotalPerdido = g.Sum(b => b.CurrentQuantity) // Cantidad restante en lotes vencidos
                })
                .OrderByDescending(x => x.TotalPerdido)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("almacenes-mas-ventas")]
        public async Task<IActionResult> GetAlmacenesMasVentas()
        {
            var userRole = GetUserRole();

            if (userRole != "0") // Solo el administrador puede ver esta información
            {
                return Forbid("No tienes permisos para acceder a este recurso.");
            }

            var result = await _context.Movements
                .Where(m => m.TypeOfMovementId == 6 && m.IsDeleted == "0") // Salida por Venta
                .GroupBy(m => m.Batch.MedicationHandlingUnit.Shelf.Warehouse.NameWarehouse)
                .Select(g => new
                {
                    Almacen = g.Key,
                    TotalVendido = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(x => x.TotalVendido)
                .Take(10) // Obtener los 10 almacenes con más ventas
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("almacenes-mas-perdidas")]
        public async Task<IActionResult> GetAlmacenesMasPerdidas()
        {
            var userRole = GetUserRole();

            if (userRole != "0") // Solo el administrador puede ver esta información
            {
                return Forbid("No tienes permisos para acceder a este recurso.");
            }

            var result = await _context.Movements
                .Where(m => m.TypeOfMovementId == 8 && m.IsDeleted == "0") // Salida por Baja (Fecha de Vencimiento)
                .GroupBy(m => m.Batch.MedicationHandlingUnit.Shelf.Warehouse.NameWarehouse)
                .Select(g => new
                {
                    Almacen = g.Key,
                    TotalPerdido = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(x => x.TotalPerdido)
                .Take(10) // Obtener los 10 almacenes con más pérdidas
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("proveedores-mas-frecuentes")]
        public async Task<IActionResult> GetProveedoresMasFrecuentes()
        {
            var result = await _context.Batches
                .Where(b => b.IsDeleted == "0" && b.Supplier != null) // Filtrar lotes activos y con proveedor asociado
                .GroupBy(b => b.Supplier.NameSupplier) // Agrupar por nombre del proveedor
                .Select(g => new
                {
                    Proveedor = g.Key, // Nombre del proveedor
                    TotalLotes = g.Count() // Contar los lotes asociados
                })
                .OrderByDescending(x => x.TotalLotes) // Ordenar por mayor cantidad de lotes
                .Take(10) // Obtener los 10 más frecuentes
                .ToListAsync();

            return Ok(result);
        }
    }
}
