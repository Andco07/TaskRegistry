using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using TaskRegistry.Frontend.ViewModels;
using TaskRegistry.Frontend.Enums;

namespace TaskRegistry.Frontend.Pages.Tasks
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            EstadosDisponibles = Enum.GetNames(typeof(EstadoTarea)).ToList();
        }

        [BindProperty]
        public TaskViewModel Task { get; set; } = new();
        public List<string> EstadosDisponibles { get; set; }
        public List<UserViewModel> Usuarios { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("TaskRegistryApi");
            var token = HttpContext.Request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"tasks/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Task = JsonSerializer.Deserialize<TaskViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
            else
            {
                return RedirectToPage("/Tasks/Index");
            }

            if (User.IsInRole("Administrador"))
            {
                var userResponse = await client.GetAsync("users");
                if (userResponse.IsSuccessStatusCode)
                {
                    var userJson = await userResponse.Content.ReadAsStringAsync();
                    Usuarios = JsonSerializer.Deserialize<List<UserViewModel>>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("TaskRegistryApi");
            var token = HttpContext.Request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Task.Id = id;
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
            Task.EstadosDisponibles = null!;

            var json = JsonSerializer.Serialize(Task);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"tasks/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Tasks/Index");
            }

            ModelState.AddModelError(string.Empty, "Error al actualizar la tarea.");
            return Page();
        }
    }
}
