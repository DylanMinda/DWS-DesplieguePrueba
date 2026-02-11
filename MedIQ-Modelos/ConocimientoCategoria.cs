using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedIQ_Modelos
{
    public class ConocimientoCategoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }
        
        [StringLength(500)]
        public string? Descripcion { get; set; }

        public virtual ICollection<ConocimientoQA> Preguntas { get; set; } = new List<ConocimientoQA>();
    }
}
