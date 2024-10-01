using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs.SupplierDTOs;
using System.Data.SqlTypes;
using System.Security.Claims;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public SuppliersController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/Suppliers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDTO>>> GetSuppliers()
        {
            return await _context.Suppliers
                 .Where(s => s.IsDeleted == "0")
                 .Select(s => new SupplierDTO
                 {
                     SupplierId = s.SupplierId,
                     NameSupplier = s.NameSupplier,
                     AddressSupplier = s.AddressSupplier,
                     CellPhoneNumber = s.CellPhoneNumber,
                     PhoneNumber = s.PhoneNumber,
                     Email = s.Email
                 })
                 .ToListAsync();
        }

        // GET: api/Suppliers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDTO>> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null || supplier.IsDeleted == "1")
            {
                return NotFound();
            }

            var supplierDTO = new SupplierDTO
            {
                SupplierId = supplier.SupplierId,
                NameSupplier = supplier.NameSupplier,
                AddressSupplier = supplier.AddressSupplier,
                PhoneNumber = supplier.PhoneNumber,
                CellPhoneNumber = supplier.CellPhoneNumber,
                Email = supplier.Email
            };

            return supplierDTO;
        }

        [HttpPost]
        public async Task<ActionResult<SupplierDTO>> PostSupplier([FromBody] CreateSupplierDTO createSupplierDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Captura el ID del usuario autenticado desde el token JWT
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

                var supplier = new Supplier
                {
                    NameSupplier = createSupplierDTO.NameSupplier,
                    AddressSupplier = createSupplierDTO.AddressSupplier,
                    PhoneNumber = createSupplierDTO.PhoneNumber,
                    CellPhoneNumber = createSupplierDTO.CellPhoneNumber,
                    Email = createSupplierDTO.Email,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsDeleted = "0"
                };

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                var supplierDTO = new SupplierDTO
                {
                    SupplierId = supplier.SupplierId,
                    NameSupplier = supplier.NameSupplier,
                    AddressSupplier = supplier.AddressSupplier,
                    PhoneNumber = supplier.PhoneNumber,
                    CellPhoneNumber = supplier.CellPhoneNumber,
                    Email = supplier.Email
                };

                return CreatedAtAction("GetSupplier", new { id = supplier.SupplierId }, supplierDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear el proveedor: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSupplier(int id, [FromBody] UpdateSupplierDTO updateSupplierDTO)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);

                if (supplier == null || supplier.IsDeleted == "1")
                {
                    return NotFound();
                }

                // Captura el ID del usuario autenticado desde el token JWT
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

                supplier.NameSupplier = updateSupplierDTO.NameSupplier;
                supplier.AddressSupplier = updateSupplierDTO.AddressSupplier;
                supplier.PhoneNumber = updateSupplierDTO.PhoneNumber;
                supplier.CellPhoneNumber = updateSupplierDTO.CellPhoneNumber;
                supplier.Email = updateSupplierDTO.Email;
                supplier.UpdatedAt = DateTime.UtcNow;
                supplier.UpdatedBy = userId;

                _context.Entry(supplier).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error de concurrencia al actualizar el proveedor: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Error de tipo SQL al actualizar el proveedor: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al actualizar el proveedor: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // DELETE: api/Suppliers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null)
                {
                    return NotFound();
                }

                supplier.IsDeleted = "1";
                supplier.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error de concurrencia al eliminar el proveedor: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Error de tipo SQL al eliminar el proveedor: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar el proveedor: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.SupplierId == id && e.IsDeleted == "0");
        }
    }
}
