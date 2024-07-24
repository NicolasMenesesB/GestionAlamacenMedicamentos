using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class Batch
{
    public int BatchId { get; set; }

    public string BatchCode { get; set; } = null!;

    public DateOnly FabricationDate { get; set; }

    public DateOnly ExpirationDate { get; set; }

    public int InitialQuantity { get; set; }

    public int CurrentQuantity { get; set; }

    public int MedicationHandlingUnitId { get; set; }

    public int SupplierId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;

    public virtual MedicationHandlingUnit MedicationHandlingUnit { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
}
