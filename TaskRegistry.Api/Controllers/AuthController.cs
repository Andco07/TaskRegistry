using Microsoft.AspNetCore.Mvc;
using TaskRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TaskRegistry.Api.Data;
using TaskRegistry.Api.Services;

namespace TaskRegistry.Api.Controllers
{
    /// <summary>
    /// Controlador para la autenticación de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        /// <summary>
        /// Constructor del controlador de autenticación.
        /// </summary>
        /// <param name="context">Contexto de base de datos.</param>
        /// <param name="jwtService">Servicio para la generación de JWT.</param>
        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Registra un nuevo usuario.
        /// </summary>
        /// <param name="usuario">Datos del usuario a registrar.</param>
        /// <returns>Respuesta con el estado del registro.</returns>
        /// <response code="200">Usuario registrado con éxito.</response>
        /// <response code="400">El correo ya está registrado.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == usuario.Correo))
                return BadRequest("El correo ya está registrado.");

            usuario.Contraseña = HashPassword(usuario.Contraseña);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado con éxito");
        }

        /// <summary>
        /// Inicia sesión con un usuario existente.
        /// </summary>
        /// <param name="usuario">Datos del usuario para iniciar sesión.</param>
        /// <returns>Token de autenticación si las credenciales son válidas.</returns>
        /// <response code="200">Inicio de sesión exitoso. Retorna el token JWT.</response>
        /// <response code="401">Correo o contraseña incorrectos.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Usuario usuario)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == usuario.Correo);

            if (user == null || !VerifyPassword(usuario.Contraseña, user.Contraseña))
                return Unauthorized("Correo o contraseña incorrectos.");

            var token = _jwtService.GenerateToken(user.Correo, user.Rol, user.Id);
            return Ok(new { Token = token });
        }

        /// <summary>
        /// Hashea una contraseña usando SHA256.
        /// </summary>
        /// <param name="password">La contraseña en texto plano.</param>
        /// <returns>La contraseña hasheada.</returns>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifica si una contraseña ingresada coincide con la contraseña almacenada.
        /// </summary>
        /// <param name="inputPassword">Contraseña ingresada por el usuario.</param>
        /// <param name="storedHash">Hash de la contraseña almacenada.</param>
        /// <returns>True si las contraseñas coinciden, de lo contrario false.</returns>
        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
}
