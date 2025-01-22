using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using TaskRegistry.Frontend.ViewModels;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        LoginData = new LoginViewModel(); 
    }

    [BindProperty]
    public LoginViewModel LoginData { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _httpClientFactory.CreateClient("TaskRegistryApi");


        var requestBody = new { correo = LoginData.Correo, contraseña = LoginData.Contraseña };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");


        var response = await client.PostAsync("auth/login", content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("token").GetString();

            HttpContext.Response.Cookies.Append("JwtToken", token!, new CookieOptions { HttpOnly = true });

            return RedirectToPage("/Tasks/Index");
        }

        LoginData.MensajeError = "Correo o contraseña incorrectos.";
        return Page();
    }
}
