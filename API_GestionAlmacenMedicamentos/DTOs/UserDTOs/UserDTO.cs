namespace API_GestionAlmacenMedicamentos.DTOs.UserDTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
