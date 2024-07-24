using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class Movement
{
    public int MovementId { get; set; }

    public int Quantity { get; set; }

    public DateOnly DateOfMoviment { get; set; }

    public int TypeOfMovementId { get; set; }

    public int BatchId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;

    public virtual Batch Batch { get; set; } = null!;

    public virtual TypeOfMovement TypeOfMovement { get; set; } = null!;
}
