namespace API_GestionAlmacenMedicamentos.DTOs.BatchDTOs
{
    public class BatchDTO
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = null!;
        public string FabricationDate { get; set; } = null!;
        public string ExpirationDate { get; set; } = null!;
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinimumStock { get; set; }
        public decimal unitPrice { get; set; } // Campo que sí está en la base de datos
        public decimal TotalPrice { get; set; } // Campo calculado, no almacenado en la base de datos
        public string MedicationName { get; set; } = null!;
        public string MedicationHandlingUnitName { get; set; }
        public string SupplierName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
