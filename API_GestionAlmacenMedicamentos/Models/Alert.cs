using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class Alert
{
    public int AlertId { get; set; }

    public string AlertType { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime GenerationDate { get; set; }

    public int BatchId { get; set; }

    public virtual Batch Batch { get; set; } = null!;
}
