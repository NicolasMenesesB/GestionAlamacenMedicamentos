using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_GestionAlmacenMedicamentos.Models
{
    public partial class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = null!;

        [Required]
        [StringLength(int.MaxValue)]
        public string Password { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = null!;

        [JsonIgnore]
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        public int CreatedBy { get; set; }

        [JsonIgnore]
        public int? UpdatedBy { get; set; }

        [JsonIgnore]
        [StringLength(1)]
        public string IsDeleted { get; set; } = "0";
    }
}