namespace API_GestionAlmacenMedicamentos.DTOs.WarehouseDTOs
{
    public class WarehouseDTO
    {
        public int WarehouseId { get; set; }
        public string NameWarehouse { get; set; } = null!;
        public string AddressWarehouse { get; set; } = null!;
    }
}
