using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedIQ_Modelos
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
