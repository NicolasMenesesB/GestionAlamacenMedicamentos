namespace API_GestionAlmacenMedicamentos.DTOs.PersonDTOs
{
    public class UpdatePersonDTO
    {
        public string? PhoneNumber { get; set; }
        public string CellPhoneNumber { get; set; } = null!;
        public string? Photo { get; set; }
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
