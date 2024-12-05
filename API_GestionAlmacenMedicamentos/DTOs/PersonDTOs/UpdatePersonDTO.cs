namespace API_GestionAlmacenMedicamentos.DTOs.PersonDTOs
{
    public class UpdatePersonDTO
    {
        public string? PhoneNumber { get; set; }
        public string CellPhoneNumber { get; set; } = null!;
        public IFormFile? Photo { get; set; }  // Manejamos el archivo de la imagen
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string WarehouseName { get; set; } = null!;
    }

}
