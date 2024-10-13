using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_GestionAlmacenMedicamentos.Models
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }

        [Required]
        [StringLength(100)]
        public string Names { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = null!;

        [StringLength(100)]
        public string? SecondLastName { get; set; }

        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(15)]
        public string CellPhoneNumber { get; set; } = null!;

        public string? Photo { get; set; }  // Guardamos la ruta o URL de la imagen

        [Required]
        [StringLength(1)]
        public string Gender { get; set; } = null!;

        [Required]
        public DateTime Birthdate { get; set; }

        [Required]
        [StringLength(150)]
        public string Address { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Ci { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Email { get; set; } = null!;

        [JsonIgnore]
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        public int CreatedBy { get; set; }

        [JsonIgnore]
        public int? UpdatedBy { get; set; }

        [StringLength(1)]
        [JsonIgnore]
        public string IsDeleted { get; set; } = "0";
    }
}