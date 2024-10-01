namespace API_GestionAlmacenMedicamentos.DTOs.TypeOfMovementDTOs
{
    public class TypeOfMovementDTO
    {
        public int TypeOfMovementId { get; set; }
        public string NameOfMovement { get; set; } = null!;
        public string? DescriptionOfMovement { get; set; }
    }
}
