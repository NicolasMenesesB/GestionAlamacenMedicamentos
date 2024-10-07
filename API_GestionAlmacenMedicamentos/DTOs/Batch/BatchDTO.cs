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
        public string MedicationHandlingUnitName { get; set; } = null!;
        public string SupplierName { get; set; } = null!;
    }
}
