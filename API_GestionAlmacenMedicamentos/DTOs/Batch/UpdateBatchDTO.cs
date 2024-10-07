namespace API_GestionAlmacenMedicamentos.DTOs.Batch
{
    public class UpdateBatchDTO
    {
        public string BatchCode { get; set; } = null!;
        public string FabricationDate { get; set; } = null!;
        public string ExpirationDate { get; set; } = null!;
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public string MedicationHandlingUnitName { get; set; } = null!;
        public string SupplierName { get; set; } = null!;
    }
}
