namespace API_GestionAlmacenMedicamentos.DTOs.ShelfDTOs
{
    public class ShelfDTO
    {
        public int ShelfId { get; set; }
        public string NameShelf { get; set; } = null!;
        public int WarehouseId { get; set; }
    }
}
