namespace API_GestionAlmacenMedicamentos.DTOs.MedicationDTOs
{
    public class MedicationDTO
    {
        public int MedicationId { get; set; }
        public string NameMedicine { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
