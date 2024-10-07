namespace API_GestionAlmacenMedicamentos.DTOs.Batch
{
    public class CreatePartialBatchDTO
    {
        // Id del medicamento existente
        public int MedicationId { get; set; }

        // Datos para MedicationHandlingUnit
        public string Concentration { get; set; } = null!;
        public int HandlingUnitId { get; set; }
        public int ShelfId { get; set; }

        // Datos para Detail_MedicationHandlingUnit
        public string StorageColdChain { get; set; } = null!;
        public string PhotoSensitiveStorage { get; set; } = null!;
        public string Controlled { get; set; } = null!;
        public string Oncological { get; set; } = null!;

        // Datos para el Batch
        public string BatchCode { get; set; } = null!;
        public string FabricationDate { get; set; }
        public string ExpirationDate { get; set; }
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public int SupplierId { get; set; }
    }
}
