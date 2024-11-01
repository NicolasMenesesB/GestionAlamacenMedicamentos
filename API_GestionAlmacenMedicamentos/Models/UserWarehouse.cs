using System.ComponentModel.DataAnnotations.Schema;

namespace API_GestionAlmacenMedicamentos.Models
{
    public class UserWarehouse
    {
        public int UserWarehouseId { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;

        public int WarehouseId { get; set; }

        public virtual Warehouse Warehouse { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public string IsDeleted { get; set; } = null!;
    }
}