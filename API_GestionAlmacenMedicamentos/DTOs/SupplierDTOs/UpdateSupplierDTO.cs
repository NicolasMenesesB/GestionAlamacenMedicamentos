namespace API_GestionAlmacenMedicamentos.DTOs.SupplierDTOs
{
    public class UpdateSupplierDTO
    {
        public string NameSupplier { get; set; } = null!;
        public string AddressSupplier { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string CellPhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
