namespace API_GestionAlmacenMedicamentos.DTOs.MedicationHandlingUnitDTOs
{
    public class UpdateMedicationHandlingUnitDTO
    {
        public string Concentration { get; set; } = null!;
        public string MedicationName { get; set; }
        public string HandlingUnitName { get; set; }
        public string ShelfName { get; set; }

        // Detail Medication Handling Unit related properties
        public string? StorageColdChain { get; set; }
        public string? PhotoSensitiveStorage { get; set; }
        public string? Controlled { get; set; }
        public string? Oncological { get; set; }
    }
}
