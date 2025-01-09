namespace API_GestionAlmacenMedicamentos.DTOs.Batch
{
    public class UpdateBatchDTO
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = null!;
        public string FabricationDate { get; set; } = null!;
        public string ExpirationDate { get; set; } = null!;
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinimumStock { get; set; }
        public decimal unitPrice { get; set; }
        public decimal? UnitPriceBonus { get; set; }
        public string MedicationName { get; set; } = null!;
        public string Concentration { get; set; } = null!;
        public string UnitMeasure { get; set; } = null!;
        public string ShelfName { get; set; } = null!;
        public string WarehouseName { get; set; } = null!;
        public string SupplierName { get; set; } = null!;
    }
}
