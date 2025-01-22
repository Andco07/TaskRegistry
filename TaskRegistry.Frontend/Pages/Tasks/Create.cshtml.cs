using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using TaskRegistry.Frontend.ViewModels;
using TaskRegistry.Frontend.Enums;

namespace TaskRegistry.Frontend.Pages.Tasks
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            Task = new TaskViewModel
            {
                EstadosDisponibles = Enum.GetNames(typeof(EstadoTarea)).ToList()
            };
        }

        [BindProperty]
        public TaskViewModel Task { get; set; } = new();

        public List<UserViewModel> Usuarios { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("TaskRegistryApi");
            var token = HttpContext.Request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }
            if (User.IsInRole("Administrador"))
            {
                var response = await client.GetAsync("users");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Usuarios = JsonSerializer.Deserialize<List<UserViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("TaskRegistryApi");
            var token = HttpContext.Request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    Task.UsuarioAsignadoId = int.Parse(userId);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No se pudo obtener el ID del usuario logueado.");
                    return Page();
                }
            }
            Task.UsuarioAsignado = null!;
            var json = JsonSerializer.Serialize(Task);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("tasks", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Tasks/Index");
            }

            ModelState.AddModelError(string.Empty, "Error al crear la tarea.");
            return Page();
        }

    }
}
