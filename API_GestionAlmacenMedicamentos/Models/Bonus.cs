namespace API_GestionAlmacenMedicamentos.Models
{
    public class Bonus
    {
        public int BonusesId { get; set; }
        public int BonusAmount { get; set; }
        public decimal BonusPrice { get; set; }
        public int BatchId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public string IsDeleted { get; set; }

        // Relaciones
        public virtual Batch Batch { get; set; }
    }
}
