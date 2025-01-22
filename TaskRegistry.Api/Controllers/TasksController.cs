using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskRegistry.Api.Data;
using TaskRegistry.Api.DTOs;
using TaskRegistry.Api.Models;

namespace TaskRegistry.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestión de tareas.
    /// </summary>
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor del controlador de tareas.
        /// </summary>
        /// <param name="context">Contexto de la base de datos.</param>
        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea una nueva tarea.
        /// </summary>
        /// <param name="tarea">Objeto de la tarea a crear.</param>
        /// <returns>La tarea creada con su ID asignado.</returns>
        /// <response code="201">Tarea creada exitosamente.</response>
        [HttpPost]
        public async Task<IActionResult> CreateTask(Tarea tarea)
        {
            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTask), new { id = tarea.Id }, tarea);
        }

        /// <summary>
        /// Obtiene una tarea específica por su ID.
        /// </summary>
        /// <param name="id">ID de la tarea a buscar.</param>
        /// <returns>La tarea solicitada.</returns>
        /// <response code="200">Tarea encontrada.</response>
        /// <response code="404">Tarea no encontrada.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null) return NotFound();
            return Ok(tarea);
        }

        /// <summary>
        /// Obtiene todas las tareas con filtros opcionales.
        /// </summary>
        /// <param name="estado">Estado de las tareas a filtrar (opcional).</param>
        /// <returns>Lista de tareas filtradas o todas las tareas disponibles.</returns>
        /// <response code="200">Lista de tareas obtenida exitosamente.</response>
        [HttpGet]
        public async Task<IActionResult> GetAllTasks([FromQuery] string? estado = null)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = User.FindFirst("UserId")?.Value;

            IQueryable<Tarea> query = _context.Tareas.Include(t => t.UsuarioAsignado);

            if (userRole != "Administrador" && int.TryParse(userId, out var userIdInt))
            {
                query = query.Where(t => t.UsuarioAsignadoId == userIdInt);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(t => t.Estado == estado);
            }

            var tareas = await query
                .Select(t => new TareaDto
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    Estado = t.Estado,
                    UsuarioAsignado = t.UsuarioAsignado != null ? t.UsuarioAsignado.Nombre : "No Asignado"
                })
                .ToListAsync();

            return Ok(tareas);
        }

        /// <summary>
        /// Actualiza una tarea existente.
        /// </summary>
        /// <param name="id">ID de la tarea a actualizar.</param>
        /// <param name="tarea">Objeto con los datos actualizados de la tarea.</param>
        /// <returns>Respuesta sin contenido si se actualiza correctamente.</returns>
        /// <response code="204">Tarea actualizada exitosamente.</response>
        /// <response code="400">El ID de la tarea no coincide con el ID proporcionado.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, Tarea tarea)
        {
            if (id != tarea.Id) return BadRequest("El ID proporcionado no coincide con el ID de la tarea.");

            _context.Entry(tarea).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Elimina una tarea existente.
        /// </summary>
        /// <param name="id">ID de la tarea a eliminar.</param>
        /// <returns>Respuesta sin contenido si se elimina correctamente.</returns>
        /// <response code="204">Tarea eliminada exitosamente.</response>
        /// <response code="404">Tarea no encontrada.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null) return NotFound();
            _context.Tareas.Remove(tarea);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
