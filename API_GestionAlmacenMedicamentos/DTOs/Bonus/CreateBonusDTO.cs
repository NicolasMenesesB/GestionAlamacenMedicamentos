namespace API_GestionAlmacenMedicamentos.DTOs.Bonus
{
    public class CreateBonusDTO
    {
        public string BatchCode { get; set; }
        public int BonusAmount { get; set; } 
        public decimal BonusPrice { get; set; } 
    }
}
