﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.DTOs.PersonDTOs;
using API_GestionAlmacenMedicamentos.Models;
using BCrypt.Net;
using System.Data.SqlTypes;
using System.Security.Claims;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Authorize(Roles = "0,1")]
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly Data.DbGestionAlmacenMedicamentosContext _context;
        private readonly IWebHostEnvironment _env;

        public PeopleController(Data.DbGestionAlmacenMedicamentosContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }

        // GET: api/People
        [HttpGet]
        [Authorize(Roles = "0,1")] // Permitir acceso solo a roles 0 (admin) y 1 (gerente)
        public async Task<ActionResult<IEnumerable<PersonDTO>>> GetPeople()
        {
            try
            {
                var people = await _context.People
                    .Where(p => p.IsDeleted == "0")
                    .Select(p => new PersonDTO
                    {
                        PersonId = p.PersonId,
                        Names = p.Names,
                        LastName = p.LastName,
                        SecondLastName = p.SecondLastName,
                        PhoneNumber = p.PhoneNumber,
                        CellPhoneNumber = p.CellPhoneNumber,
                        Photo = p.Photo,
                        Gender = p.Gender,
                        Birthdate = p.Birthdate,
                        Address = p.Address,
                        Ci = p.Ci,
                        Email = p.Email
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = people });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetPerson(int id)
        {
            // Buscar la persona por ID
            var person = await _context.People.FindAsync(id);

            if (person == null || person.IsDeleted == "1")
            {
                return NotFound(new { success = false, message = "Persona no encontrada" });
            }

            // Obtener el rol del usuario asociado a esta persona
            var role = await _context.Users
                .Where(u => u.UserId == person.PersonId && u.IsDeleted == "0")
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            // Obtener el nombre del almacén asignado al usuario
            var warehouseName = await _context.UserWarehouses
                .Where(uw => uw.UserId == person.PersonId && uw.IsDeleted == "0")
                .Join(_context.Warehouses, uw => uw.WarehouseId, w => w.WarehouseId, (uw, w) => new { w.NameWarehouse })
                .Select(w => w.NameWarehouse)
                .FirstOrDefaultAsync();

            // Construir el DTO para la respuesta
            var personDTO = new PersonDTO
            {
                PersonId = person.PersonId,
                Names = person.Names,
                LastName = person.LastName,
                SecondLastName = person.SecondLastName,
                PhoneNumber = person.PhoneNumber,
                CellPhoneNumber = person.CellPhoneNumber,
                Photo = person.Photo,
                Gender = person.Gender,
                Birthdate = person.Birthdate,
                Address = person.Address,
                Ci = person.Ci,
                Email = person.Email,
                Role = role, // Rol del usuario
                WarehouseName = warehouseName // Nombre del almacén
            };

            return Ok(new { success = true, data = personDTO });
        }

        // POST: api/People
        [HttpPost]
        [Authorize(Roles = "0,1")] // Permitir solo a roles admin y gerente crear personas
        public async Task<ActionResult> PostPerson([FromForm] CreatePersonDTO createPersonDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });
            }

            string photoUrl = null;

            // Guardar foto si existe
            if (createPersonDTO.Photo != null)
            {
                try
                {
                    var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + createPersonDTO.Photo.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await createPersonDTO.Photo.CopyToAsync(fileStream);
                    }

                    photoUrl = Path.Combine("Uploads", uniqueFileName);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Error al guardar la foto", details = ex.Message });
                }
            }

            // Crear el objeto Persona
            var person = new Person
            {
                Names = createPersonDTO.Names,
                LastName = createPersonDTO.LastName,
                SecondLastName = createPersonDTO.SecondLastName,
                PhoneNumber = createPersonDTO.PhoneNumber,
                CellPhoneNumber = createPersonDTO.CellPhoneNumber,
                Photo = photoUrl,
                Gender = createPersonDTO.Gender,
                Birthdate = createPersonDTO.Birthdate,
                Address = createPersonDTO.Address,
                Ci = createPersonDTO.Ci,
                Email = createPersonDTO.Email,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId(),
                IsDeleted = "0"
            };

            try
            {
                _context.People.Add(person);
                await _context.SaveChangesAsync();

                // Crear usuario relacionado
                var userName = GenerateUserName(person.Names, person.LastName, person.Ci);
                var password = GeneratePassword(person.Names, person.LastName, person.Ci);
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var user = new User
                {
                    UserId = person.PersonId,
                    UserName = userName,
                    Password = hashedPassword,
                    Role = createPersonDTO.Role,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserId(),
                    IsDeleted = "0"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Asignar almacén si corresponde
                var warehouse = await _context.Warehouses
                    .FirstOrDefaultAsync(w => w.NameWarehouse == createPersonDTO.WarehouseName);

                if (warehouse == null)
                {
                    return BadRequest(new { success = false, message = "El almacén especificado no existe" });
                }

                // Verificar que no exista ya un jefe de almacén
                if (user.Role == "1")
                {
                    var existingManager = await _context.UserWarehouses
                        .Include(uw => uw.User)
                        .Where(uw => uw.WarehouseId == warehouse.WarehouseId && uw.User.Role == "1" && uw.IsDeleted == "0")
                        .FirstOrDefaultAsync();

                    if (existingManager != null)
                    {
                        return BadRequest(new { success = false, message = "Ya existe un jefe de almacén para este almacén." });
                    }
                }

                var userWarehouse = new UserWarehouse
                {
                    UserId = user.UserId,
                    WarehouseId = warehouse.WarehouseId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserId(),
                    IsDeleted = "0"
                };

                _context.UserWarehouses.Add(userWarehouse);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPerson", new { id = person.PersonId }, new { success = true, data = person });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Error al crear la persona", details = ex.Message });
            }
        }

        // PUT: api/People/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, [FromForm] UpdatePersonDTO updatePersonDTO)
        {
            var person = await _context.People.FindAsync(id);

            if (person == null || person.IsDeleted == "1")
            {
                return NotFound(new { success = false, message = "Persona no encontrada" });
            }

            // Actualizar la foto si se proporciona una nueva
            if (updatePersonDTO.Photo != null)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + updatePersonDTO.Photo.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder); // Asegurar que el directorio exista

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await updatePersonDTO.Photo.CopyToAsync(fileStream);
                }

                person.Photo = Path.Combine("Uploads", uniqueFileName);
            }

            // Actualizar los datos personales
            person.PhoneNumber = updatePersonDTO.PhoneNumber;
            person.CellPhoneNumber = updatePersonDTO.CellPhoneNumber;
            person.Address = updatePersonDTO.Address;
            person.Email = updatePersonDTO.Email;
            person.UpdatedAt = DateTime.UtcNow;
            person.UpdatedBy = GetCurrentUserId();

            // Actualizar el rol
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.IsDeleted == "0");
            if (user != null)
            {
                // Verificar que no exista ya un jefe de almacén
                if (user.Role != updatePersonDTO.Role && updatePersonDTO.Role == "1")
                {
                    var warehouse = await _context.UserWarehouses
                        .Where(uw => uw.UserId == id && uw.IsDeleted == "0")
                        .Select(uw => uw.Warehouse)
                        .FirstOrDefaultAsync();

                    if (warehouse != null)
                    {
                        var existingManager = await _context.UserWarehouses
                            .Include(uw => uw.User)
                            .Where(uw => uw.WarehouseId == warehouse.WarehouseId && uw.User.Role == "1" && uw.IsDeleted == "0")
                            .FirstOrDefaultAsync();

                        if (existingManager != null)
                        {
                            return BadRequest(new { success = false, message = "Ya existe un jefe de almacén para este almacén." });
                        }
                    }
                }

                user.Role = updatePersonDTO.Role;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = GetCurrentUserId();
            }

            // Actualizar el almacén
            if (!string.IsNullOrEmpty(updatePersonDTO.WarehouseName))
            {
                var warehouse = await _context.Warehouses
                    .FirstOrDefaultAsync(w => w.NameWarehouse == updatePersonDTO.WarehouseName && w.IsDeleted == "0");

                if (warehouse == null)
                {
                    return BadRequest(new { success = false, message = "El almacén especificado no existe" });
                }

                var userWarehouse = await _context.UserWarehouses
                    .FirstOrDefaultAsync(uw => uw.UserId == id && uw.IsDeleted == "0");

                if (userWarehouse != null)
                {
                    userWarehouse.WarehouseId = warehouse.WarehouseId;
                    userWarehouse.UpdatedAt = DateTime.UtcNow;
                    userWarehouse.UpdatedBy = GetCurrentUserId();
                }
                else
                {
                    _context.UserWarehouses.Add(new UserWarehouse
                    {
                        UserId = id,
                        WarehouseId = warehouse.WarehouseId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = GetCurrentUserId(),
                        IsDeleted = "0"
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Error al actualizar la persona", details = ex.Message });
            }

            return NoContent();
        }

        // GET: api/CheckWarehouseHasChief
        [HttpGet("CheckWarehouseHasChief/{warehouseName}/{currentUserId?}")]
        public async Task<IActionResult> CheckWarehouseHasChief(string warehouseName, int? currentUserId = null)
        {
            try
            {
                // Buscar el almacén por su nombre
                var warehouse = await _context.Warehouses
                    .Where(w => w.NameWarehouse == warehouseName && w.IsDeleted == "0")
                    .FirstOrDefaultAsync();

                if (warehouse == null)
                {
                    return NotFound(new { hasChief = false, currentChiefId = (int?)null, message = "Almacén no encontrado." });
                }

                // Verificar si ya existe un jefe asignado al almacén
                var currentChief = await _context.Users
                    .Join(_context.UserWarehouses, u => u.UserId, uw => uw.UserId, (u, uw) => new { u, uw })
                    .Where(joined => joined.u.Role == "1" &&
                                     joined.uw.WarehouseId == warehouse.WarehouseId &&
                                     joined.u.IsDeleted == "0" &&
                                     joined.uw.IsDeleted == "0")
                    .Select(joined => joined.u.UserId)
                    .FirstOrDefaultAsync();

                // Si el jefe actual es el mismo que se está editando, no hay problema
                if (currentChief != 0 && currentChief == currentUserId)
                {
                    return Ok(new { hasChief = false, currentChiefId = currentChief });
                }

                // Retornar el resultado con el ID del jefe actual (si existe)
                return Ok(new
                {
                    hasChief = currentChief != 0, // Verificar si existe un jefe
                    currentChiefId = currentChief != 0 ? currentChief : (int?)null // ID del jefe actual o null
                });
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = $"Error al verificar el almacén: {ex.Message}"
                });
            }
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == GenerateUserName(person.Names, person.LastName, person.Ci));

            // Implementar eliminación lógica para la persona
            person.IsDeleted = "1";
            person.UpdatedAt = DateTime.UtcNow;

            if (user != null)
            {
                // Implementar eliminación lógica para el usuario si existe
                user.IsDeleted = "1";
                user.UpdatedAt = DateTime.UtcNow;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Concurrency error: {ex.Message}");
            }
            catch (SqlTypeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"SQL type error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message} - {ex.InnerException?.Message}");
            }

            return NoContent();
        }

        [HttpGet("GetDeletedPeople")]
        public async Task<IActionResult> GetDeletedPeople()
        {
            try
            {
                var deletedPeople = await _context.People
                    .Where(p => p.IsDeleted == "1")
                    .Select(p => new PersonDTO
                    {
                        PersonId = p.PersonId,
                        Names = p.Names,
                        LastName = p.LastName,
                        SecondLastName = p.SecondLastName,
                        PhoneNumber = p.PhoneNumber,
                        CellPhoneNumber = p.CellPhoneNumber,
                        Email = p.Email
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = deletedPeople });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("RestorePerson/{id}")]
        public async Task<IActionResult> RestorePerson(int id)
        {
            try
            {
                var person = await _context.People.FindAsync(id);

                if (person == null || person.IsDeleted == "0")
                {
                    return NotFound(new { success = false, message = "Persona no encontrada o ya está activa." });
                }

                // Restaurar persona
                person.IsDeleted = "0";
                person.UpdatedAt = DateTime.UtcNow;
                person.UpdatedBy = GetCurrentUserId(); // ID del usuario que realiza la acción

                // Restaurar usuario asociado
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
                if (user != null)
                {
                    user.IsDeleted = "0";
                    user.UpdatedAt = DateTime.UtcNow;
                    user.UpdatedBy = GetCurrentUserId();
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Persona restaurada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        private string GenerateUserName(string names, string lastName, string ci)
        {
            return names.Substring(0, 2).ToLower() + lastName.Substring(0, 2).ToLower() + ci.Substring(0, 3);
        }

        private string GeneratePassword(string names, string lastName, string ci)
        {
            return names.Substring(0, 1).ToUpper() +
                   names.Substring(1, 1).ToLower() +
                   lastName.Substring(0, 1).ToUpper() +
                   lastName.Substring(1, 1).ToLower() +
                   ci.Substring(0, 4);
        }

        // Verificar si el CI ya está registrado
        [HttpGet("CheckCIExists/{ci}")]
        public IActionResult CheckCIExists(string ci)
        {
            var ciExists = _context.People
                .Any(p => p.Ci == ci && p.IsDeleted == "0");
            return Ok(new { exists = ciExists });
        }

        // Verificar si el correo ya está registrado
        [HttpGet("CheckEmailExists/{email}")]
        public IActionResult CheckEmailExists(string email)
        {
            var emailExists = _context.People
                .Any(p => p.Email.ToLower() == email.ToLower() && p.IsDeleted == "0");
            return Ok(new { exists = emailExists });
        }

        private bool PersonExists(int id)
        {
            return _context.People.Any(e => e.PersonId == id && e.IsDeleted == "0");
        }
    }
}
