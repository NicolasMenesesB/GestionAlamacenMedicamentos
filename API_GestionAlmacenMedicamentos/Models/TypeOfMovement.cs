using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class TypeOfMovement
{
    public int TypeOfMovementId { get; set; }

    /// <summary>
    /// Entrada por compra a los proveedores
    /// Entrada por cambio de producto
    /// Entrada por bonificacion
    /// 
    /// Salida para venta
    /// Salida por devolucion
    /// Salida por baja (Vencimineto)
    /// 
    /// 
    /// </summary>
    public string NameOfMovement { get; set; } = null!;

    public string? DescriptionOfMovement { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;
}
