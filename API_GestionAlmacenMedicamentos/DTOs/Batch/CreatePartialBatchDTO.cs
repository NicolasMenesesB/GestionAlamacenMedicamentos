namespace API_GestionAlmacenMedicamentos.DTOs.Batch
{
    public class CreatePartialBatchDTO
    {
        public int MedicationId { get; set; } // ID del medicamento existente
        public string Concentration { get; set; } = null!; // Concentración del medicamento
        public int HandlingUnitId { get; set; } // ID de la unidad de manejo
        public int ShelfId { get; set; } // ID del estante
        public string BatchCode { get; set; } = null!; // Código del lote
        public string FabricationDate { get; set; } = null!; // Fecha de fabricación (en formato YYYY-MM-DD)
        public string ExpirationDate { get; set; } = null!; // Fecha de expiración (en formato YYYY-MM-DD)
        public int InitialQuantity { get; set; } // Cantidad inicial del lote
        public int CurrentQuantity { get; set; } // Cantidad actual del lote
        public int MinimumStock { get; set; } // Stock mínimo requerido del lote
        public int SupplierId { get; set; } // ID del proveedor
        public string StorageColdChain { get; set; } = "0"; // Indica si necesita cadena de frío (0 o 1)
        public string PhotoSensitiveStorage { get; set; } = "0"; // Indica si es fotosensible (0 o 1)
        public string Controlled { get; set; } = "0"; // Indica si es controlado (0 o 1)
        public string Oncological { get; set; } = "0"; // Indica si es oncológico (0 o 1)
        public string NameOfMovement { get; set; } = null!; // Tipo de movimiento (entrada o salida)
        public string DateOfMoviment { get; set; } = null!; // Fecha del movimiento
        public int Quantity { get; set; } // Cantidad del movimiento (corresponde a la cantidad inicial)
    }

}
