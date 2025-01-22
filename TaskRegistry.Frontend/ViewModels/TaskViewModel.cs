namespace TaskRegistry.Frontend.ViewModels
{
    public class TaskViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty ;
        public string Estado { get; set; } = string.Empty;
        public int UsuarioAsignadoId { get; set; }
        public string UsuarioAsignado { get; set; } = string.Empty;
        public List<string> EstadosDisponibles { get; set; } = new();
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
