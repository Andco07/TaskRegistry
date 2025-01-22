using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using TaskRegistry.Frontend.Enums;
using TaskRegistry.Frontend.ViewModels;

namespace TaskRegistry.Frontend.Pages.Tasks
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            EstadosDisponibles = Enum.GetNames(typeof(EstadoTarea)).ToList();
        }

        public List<TaskViewModel> Tareas { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string EstadoFiltro { get; set; } = string.Empty;
        public List<string> EstadosDisponibles { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("TaskRegistryApi");

            var token = HttpContext.Request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var query = string.IsNullOrEmpty(EstadoFiltro) ? "tasks" : $"tasks?estado={EstadoFiltro}";
            var response = await client.GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Tareas = System.Text.Json.JsonSerializer.Deserialize<List<TaskViewModel>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }

            return Page();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("TaskRegistryApi");
            var token = HttpContext.Request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"tasks/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, "Error al eliminar la tarea.");
            return Page();
        }
    }
}
