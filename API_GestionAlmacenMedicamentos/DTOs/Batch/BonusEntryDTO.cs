namespace API_GestionAlmacenMedicamentos.DTOs.Batch
{
    public class BonusEntryDTO
    {
        public string BatchCode { get; set; } = null!; 
        public int BonusQuantity { get; set; }        
        public decimal? UnitPriceBonus { get; set; }
        public string NameOfMovement { get; set; } = null!;
    }
}
