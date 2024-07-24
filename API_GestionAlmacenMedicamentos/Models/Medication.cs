using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class Medication
{
    public int MedicationId { get; set; }

    public string NameMedicine { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;
}
