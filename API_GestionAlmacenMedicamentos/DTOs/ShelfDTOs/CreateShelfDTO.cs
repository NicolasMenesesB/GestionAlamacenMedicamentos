namespace API_GestionAlmacenMedicamentos.DTOs.ShelfDTOs
{
    public class CreateShelfDTO
    {
        public string NameShelf { get; set; } = null!;
        public int WarehouseId { get; set; }
    }
}
