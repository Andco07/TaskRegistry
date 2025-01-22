using Microsoft.EntityFrameworkCore;
using TaskRegistry.Api.Data;
using TaskRegistry.Api.Models;
using Xunit;

namespace TaskRegistry.Tests.Services
{
    public class TareasServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public TareasServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Tareas.Add(new Tarea { Id = 1, Titulo = "Tarea 1", Descripcion = "Descripción 1", Estado = "Pendiente", UsuarioAsignadoId = 1 });
            _context.Tareas.Add(new Tarea { Id = 2, Titulo = "Tarea 2", Descripcion = "Descripción 2", Estado = "En Proceso", UsuarioAsignadoId = 2 });
            _context.SaveChanges();
        }

        [Fact]
        public void ObtenerTareas_DevuelveTodasLasTareas()
        {
            var tareas = _context.Tareas.ToList();

            Assert.Equal(2, tareas.Count);
            Assert.Contains(tareas, t => t.Titulo == "Tarea 1");
            Assert.Contains(tareas, t => t.Titulo == "Tarea 2");
        }

        [Fact]
        public void CrearTarea_AgregaNuevaTarea()
        {

            var nuevaTarea = new Tarea { Id = 3, Titulo = "Tarea 3", Descripcion = "Descripción 3", Estado = "Pendiente", UsuarioAsignadoId = 1 };

            _context.Tareas.Add(nuevaTarea);
            _context.SaveChanges();

            var tarea = _context.Tareas.Find(3);
            Assert.NotNull(tarea);
            Assert.Equal("Tarea 3", tarea.Titulo);
        }

        [Fact]
        public void FiltrarTareasPorEstado_DevuelveSoloPendientes()
        {
            var tareasPendientes = _context.Tareas.Where(t => t.Estado == "Pendiente").ToList();

            Assert.Single(tareasPendientes);
            Assert.Equal("Tarea 1", tareasPendientes[0].Titulo);
        }

        [Fact]
        public void EliminarTarea_RemueveTareaDelContexto()
        {
            var tarea = _context.Tareas.Find(1);

            if (tarea != null)
            {
                _context.Tareas.Remove(tarea);
                _context.SaveChanges();
            }

            var tareaEliminada = _context.Tareas.Find(1);
            Assert.Null(tareaEliminada);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
