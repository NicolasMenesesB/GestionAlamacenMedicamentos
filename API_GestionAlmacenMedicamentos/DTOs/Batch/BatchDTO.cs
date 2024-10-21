namespace API_GestionAlmacenMedicamentos.DTOs.BatchDTOs
{
     public class BatchDTO
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = null!;
        public string FabricationDate { get; set; } = null!;  // Representado como string para formateo flexible
        public string ExpirationDate { get; set; } = null!;   // Igual que la fecha de fabricación
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinimumStock { get; set; }                 // Nuevo campo de MinimumStock
        public string MedicationHandlingUnitName { get; set; }
        public string SupplierName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
