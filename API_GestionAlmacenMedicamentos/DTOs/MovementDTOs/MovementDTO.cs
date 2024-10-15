namespace API_GestionAlmacenMedicamentos.DTOs.MovementDTOs
{
    public class MovementDTO
    {
        public int MovementId { get; set; }
        public int Quantity { get; set; }
        public string DateOfMoviment { get; set; }  // Formateado como cadena si es necesario para el frontend
        public string NameOfMovement { get; set; }  // Nombre del tipo de movimiento en lugar del ID
        public string BatchCode { get; set; }       // Código de lote en lugar del ID
    }

}
