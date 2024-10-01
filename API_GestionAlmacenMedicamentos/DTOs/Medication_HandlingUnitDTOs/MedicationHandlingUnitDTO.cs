namespace API_GestionAlmacenMedicamentos.DTOs.MedicationHandlingUnitDTOs
{
    public class MedicationHandlingUnitDTO
    {
        public int MedicationHandlingUnitId { get; set; }
        public string Concentration { get; set; } = null!;

        // Agregando propiedades para los nombres en lugar de los IDs
        public string MedicationName { get; set; } = null!;
        public string HandlingUnitName { get; set; } = null!;
        public string ShelfName { get; set; } = null!;

        // Detail Medication Handling Unit related properties
        public string? StorageColdChain { get; set; }
        public string? PhotoSensitiveStorage { get; set; }
        public string? Controlled { get; set; }
        public string? Oncological { get; set; }
    }
}
