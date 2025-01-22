using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using TaskRegistry.Frontend.ViewModels;

namespace TaskRegistry.Frontend.Pages.Users
{
    public class UsersIndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UsersIndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<UserViewModel> Usuarios { get; set; } = new();

        public string MensajeError { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("TaskRegistryApi");

            var token = HttpContext.Request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("users");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Usuarios = JsonSerializer.Deserialize<List<UserViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MensajeError = "No tienes permisos para acceder a esta sección.";
                return Page();
            }
            else
            {
                MensajeError = "Error al obtener los usuarios.";
            }

            return Page();
        }
    }
}
