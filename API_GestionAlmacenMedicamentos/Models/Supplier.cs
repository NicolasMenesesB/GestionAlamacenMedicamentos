using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class Supplier
{
    public int SupplierId { get; set; }

    public string NameSupplier { get; set; } = null!;

    public string AddressSupplier { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string CellPhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;
}
