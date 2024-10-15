namespace API_GestionAlmacenMedicamentos.DTOs.MovementDTOs
{
    public class CreateMovementDTO
    {
        public int Quantity { get; set; }
        public string DateOfMoviment { get; set; }  // Fecha en formato string si es necesario para el frontend
        public string NameOfMovement { get; set; }  // Nombre del tipo de movimiento
        public string BatchCode { get; set; }       // Código del lote
    }

}
