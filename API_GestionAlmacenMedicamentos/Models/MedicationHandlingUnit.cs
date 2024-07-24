using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class MedicationHandlingUnit
{
    public int MedicationHandlingUnitId { get; set; }

    public string Concentration { get; set; } = null!;

    public int MedicationId { get; set; }

    public int HandlingUnitId { get; set; }

    public int ShelfId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;

    public virtual DetailMedicationHandlingUnit? DetailMedicationHandlingUnit { get; set; }

    public virtual HandlingUnit HandlingUnit { get; set; } = null!;

    public virtual Medication Medication { get; set; } = null!;

    public virtual Shelf Shelf { get; set; } = null!;
}
