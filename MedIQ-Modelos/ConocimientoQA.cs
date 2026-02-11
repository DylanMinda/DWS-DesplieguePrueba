using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedIQ_Modelos
{
    public class ConocimientoQA
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Pregunta { get; set; }

        [Required]
        public string Respuesta { get; set; }

        [StringLength(500)]
        public string? Keywords { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual ConocimientoCategoria Categoria { get; set; }

        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual ConocimientoQA? Parent { get; set; }
        public virtual ICollection<ConocimientoQA> SubPreguntas { get; set; } = new List<ConocimientoQA>();

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
    }
}
