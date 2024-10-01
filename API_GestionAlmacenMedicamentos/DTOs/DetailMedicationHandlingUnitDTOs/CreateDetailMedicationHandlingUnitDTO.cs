namespace API_GestionAlmacenMedicamentos.DTOs.DetailMedicationHandlingUnitDTOs
{
    public class CreateDetailMedicationHandlingUnitDTO
    {
        public string StorageColdChain { get; set; }
        public string PhotoSensitiveStorage { get; set; }
        public string Controlled { get; set; }
        public string Oncological { get; set; }
    }
}
