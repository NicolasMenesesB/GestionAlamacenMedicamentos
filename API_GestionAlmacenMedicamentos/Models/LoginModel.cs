using System.ComponentModel.DataAnnotations;

namespace API_GestionAlmacenMedicamentos.Models
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
