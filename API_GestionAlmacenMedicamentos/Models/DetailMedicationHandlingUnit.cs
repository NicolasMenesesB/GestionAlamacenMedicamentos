using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class DetailMedicationHandlingUnit
{
    public int DetailMedicationHandlingUnitId { get; set; }

    public string? StorageColdChain { get; set; }

    public string? PhotoSensitiveStorage { get; set; }

    public string? Controlled { get; set; }

    public string? Oncological { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;

    [JsonIgnore]
    public virtual MedicationHandlingUnit DetailMedicationHandlingUnitNavigation { get; set; } = null!;
}
