﻿namespace TaskRegistry.Api.DTOs
{
    public class TareaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string UsuarioAsignado { get; set; } = string.Empty;
    }
}
