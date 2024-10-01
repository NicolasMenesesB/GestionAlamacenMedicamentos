using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_GestionAlmacenMedicamentos.Data;
using API_GestionAlmacenMedicamentos.Models;
using API_GestionAlmacenMedicamentos.DTOs;
using BCrypt.Net;
using System.Data.SqlTypes;
using API_GestionAlmacenMedicamentos.DTOs.PersonDTOs;

namespace API_GestionAlmacenMedicamentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly DbGestionAlmacenMedicamentosContext _context;

        public PeopleController(DbGestionAlmacenMedicamentosContext context)
        {
            _context = context;
        }

        // GET: api/People
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonDTO>>> GetPeople()
        {
            return await _context.People
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
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonDTO>> GetPerson(int id)
        {
            var person = await _context.People.FindAsync(id);

            if (person == null || person.IsDeleted == "1")
            {
                return NotFound();
            }

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
                Email = person.Email
            };

            return personDTO;
        }

        // POST: api/People
        [HttpPost]
        public async Task<ActionResult<PersonDTO>> PostPerson([FromBody] CreatePersonDTO createPersonDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = new Person
            {
                Names = createPersonDTO.Names,
                LastName = createPersonDTO.LastName,
                SecondLastName = createPersonDTO.SecondLastName,
                PhoneNumber = createPersonDTO.PhoneNumber,
                CellPhoneNumber = createPersonDTO.CellPhoneNumber,
                Photo = createPersonDTO.Photo,
                Gender = createPersonDTO.Gender,
                Birthdate = createPersonDTO.Birthdate,
                Address = createPersonDTO.Address,
                Ci = createPersonDTO.Ci,
                Email = createPersonDTO.Email,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1,
                IsDeleted = "0"
            };

            // Agregar la persona al contexto para obtener el ID autogenerado
            _context.People.Add(person);
            await _context.SaveChangesAsync();

            // Generar el UserName y la Password a partir de las reglas dadas
            string userName = GenerateUserName(person.Names, person.LastName, person.Ci);
            string plainPassword = GeneratePassword(person.Names, person.LastName, person.Ci);

            // Encriptar la contraseña usando BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            // Crear un nuevo usuario con el ID autogenerado por la base de datos
            var user = new User
            {
                UserName = userName,
                Password = hashedPassword,
                Role = createPersonDTO.Role,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1,
                IsDeleted = "0"
            };

            // Agregar el usuario al contexto
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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
                Email = person.Email
            };

            return CreatedAtAction("GetPerson", new { id = person.PersonId }, personDTO);
        }

        // PUT: api/People/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, [FromBody] UpdatePersonDTO updatePersonDTO)
        {
            var person = await _context.People.FindAsync(id);

            if (person == null || person.IsDeleted == "1")
            {
                return NotFound();
            }

            person.PhoneNumber = updatePersonDTO.PhoneNumber;
            person.CellPhoneNumber = updatePersonDTO.CellPhoneNumber;
            person.Photo = updatePersonDTO.Photo;
            person.Address = updatePersonDTO.Address;
            person.Email = updatePersonDTO.Email;
            person.UpdatedAt = DateTime.UtcNow;
            person.UpdatedBy = 1; // Ajusta esto según tu lógica de negocio

            _context.Entry(person).State = EntityState.Modified;

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

        private bool PersonExists(int id)
        {
            return _context.People.Any(e => e.PersonId == id && e.IsDeleted == "0");
        }
    }
}
