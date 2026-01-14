namespace DWS.Models
{
    public class Mensaje
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public string? ImagenUrl { get; set; }
        public bool EsIA {  get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public int ChatSessionId { get; set; }

    }
}
