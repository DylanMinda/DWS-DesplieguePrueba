namespace DWS.Models
{
    public class ChatSession
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "Nuevo Chat";
        public DateTime FechaCreacion { get; set; }= DateTime.UtcNow;
        public int UsuarioId { get; set; }
    }
}
