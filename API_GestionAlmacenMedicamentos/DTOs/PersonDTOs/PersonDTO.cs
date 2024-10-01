namespace API_GestionAlmacenMedicamentos.DTOs.PersonDTOs
{
    public class PersonDTO
    {
        public int PersonId { get; set; }
        public string Names { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? SecondLastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string CellPhoneNumber { get; set; } = null!;
        public string? Photo { get; set; }
        public string Gender { get; set; } = null!;
        public DateTime Birthdate { get; set; }
        public string Address { get; set; } = null!;
        public string Ci { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
