using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedIQ_Modelos
{ 
    public class ChatSession
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "Nuevo Chat";
        public DateTime FechaCreacion { get; set; }= DateTime.UtcNow;
        public int UsuarioId { get; set; }
    }
}
