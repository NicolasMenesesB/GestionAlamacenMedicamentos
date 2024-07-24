using System;
using System.Collections.Generic;

namespace API_GestionAlmacenMedicamentos.Models;

public partial class Person
{
    public int PersonId { get; set; }

    public string Names { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? SecondLastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string CellPhoneNumber { get; set; } = null!;

    public string Photo { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateOnly Birthdate { get; set; }

    public string Address { get; set; } = null!;

    public string Ci { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public string IsDeleted { get; set; } = null!;

    public virtual User? User { get; set; }
}
