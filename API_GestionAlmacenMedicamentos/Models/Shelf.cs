using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class Shelf
{
    public int ShelfId { get; set; }

    public string NameShelf { get; set; } = null!;

    public int WarehouseId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
