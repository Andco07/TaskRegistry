using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using TaskRegistry.Frontend.Enums;
using TaskRegistry.Frontend.ViewModels;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        RegisterData = new RegisterViewModel();

        RegisterData.RolesDisponibles = Enum.GetNames(typeof(RolesEnum)).ToList();
    }

    [BindProperty]
    public RegisterViewModel RegisterData { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _httpClientFactory.CreateClient("TaskRegistryApi");
        var requestBody = new
        {
            nombre = RegisterData.Nombre,
            correo = RegisterData.Correo,
            contraseña = RegisterData.Contraseña,
            rol = RegisterData.RolSeleccionado
        };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("auth/register", content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("/Auth/Login");
        }
        RegisterData.MensajeError = "Error al registrar el usuario. Por favor, inténtalo de nuevo.";
        return Page();
    }
}
