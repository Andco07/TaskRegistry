namespace TaskRegistry.Frontend.ViewModels
{
    public class RegisterViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contraseña { get; set; } = string.Empty;
        public string RolSeleccionado { get; set; } = string.Empty;
        public List<string> RolesDisponibles { get; set; } = new List<string>();
        public string MensajeError { get; set; } = string.Empty;
    }
}
