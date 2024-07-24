using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class HandlingUnit
{
    public int HandlingUnitId { get; set; }

    public string NameUnit { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;
}
