using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskRegistry.Api.Controllers;
using TaskRegistry.Api.Data;
using TaskRegistry.Api.DTOs;
using TaskRegistry.Api.Models;
using Xunit;

namespace TaskRegistry.Tests.Controllers
{
    public class TasksControllerTests
    {
        private ApplicationDbContext _context;
        private TasksController _controller;

        public TasksControllerTests()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var dbName = $"TestDatabase_{System.Guid.NewGuid()}";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Tareas.Add(new Tarea { Id = 1, Titulo = "Tarea 1", Descripcion = "Descripción 1", Estado = "Pendiente", UsuarioAsignadoId = 1 });
            _context.Tareas.Add(new Tarea { Id = 2, Titulo = "Tarea 2", Descripcion = "Descripción 2", Estado = "EnProceso", UsuarioAsignadoId = 1 });
            _context.Tareas.Add(new Tarea { Id = 3, Titulo = "Tarea 3", Descripcion = "Descripción 3", Estado = "Completada", UsuarioAsignadoId = 2 });
            _context.Usuarios.Add(new Usuario { Id = 1, Nombre = "Usuario 1" });
            _context.Usuarios.Add(new Usuario { Id = 2, Nombre = "Usuario 2" });
            _context.SaveChanges();

            _controller = new TasksController(_context);
        }

        private void SetupControllerUser(string role, int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, role),
                new Claim("UserId", userId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAllTasks_AsAdmin_ReturnsAllTasks()
        {
            SetupControllerUser("Administrador", 1);

            var result = await _controller.GetAllTasks() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var tareas = result.Value as IEnumerable<dynamic>;
            Assert.NotNull(tareas);

            Assert.Equal(3, tareas.Count());
        }

        [Fact]
        public async Task GetAllTasks_AsAdminWithStateFilter_ReturnsFilteredTasks()
        {
            SetupControllerUser("Administrador", 1);

            var result = await _controller.GetAllTasks("Pendiente") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var tareas = result.Value as IEnumerable<dynamic>;
            Assert.NotNull(tareas);
            Assert.Single(tareas);
            Assert.Equal("Tarea 1", tareas.First().Titulo);
        }

        [Fact]
        public async Task GetAllTasks_AsUser_ReturnsOnlyAssignedTasks()
        {
            SetupControllerUser("Usuario", 1);

            var result = await _controller.GetAllTasks() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var tareas = result.Value as IEnumerable<dynamic>;
            Assert.NotNull(tareas);
            Assert.Equal(2, tareas.Count());
            Assert.Contains(tareas, t => t.Titulo == "Tarea 1");
            Assert.Contains(tareas, t => t.Titulo == "Tarea 2");
        }

        [Fact]
        public async Task GetAllTasks_AsUserWithStateFilter_ReturnsFilteredAssignedTasks()
        {
            SetupControllerUser("Usuario", 1);

            var result = await _controller.GetAllTasks("Pendiente") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var tareas = result.Value as IEnumerable<dynamic>;
            Assert.NotNull(tareas);
            Assert.Single(tareas);
            Assert.Equal("Tarea 1", tareas.First().Titulo);
        }
    }
}
