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
        public decimal unitPrice { get; set; } // Precio unitario
        public decimal? UnitPriceBonus { get; set; } // Precio de bonificación
        public decimal TotalPrice => InitialQuantity * unitPrice; // Calculado
        public string MedicationName { get; set; } = null!;
        public string Concentration { get; set; } = null!; // Nueva propiedad
        public string UnitMeasure { get; set; } = null!; // Nueva propiedad
        public string ShelfName { get; set; } = null!; // Nueva propiedad
        public string WarehouseName { get; set; } = null!; // Nueva propiedad
        public string SupplierName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
