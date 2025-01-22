using System.ComponentModel.DataAnnotations;

namespace TaskRegistry.Api.Models
{
    public class Tarea
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [RegularExpression("(Pendiente|EnProceso|Completada)")]
        public string Estado { get; set; } = "Pendiente";
        public int UsuarioAsignadoId { get; set; }
        public Usuario? UsuarioAsignado { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}
