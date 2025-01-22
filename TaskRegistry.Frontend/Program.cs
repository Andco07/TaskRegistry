using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var backendApiUrl = builder.Configuration["BackendApi:BaseUrl"];
builder.Services.AddHttpClient("TaskRegistryApi", client =>
{
    client.BaseAddress = new Uri(backendApiUrl ?? throw new InvalidOperationException("BackendApi:BaseUrl no está configurado."));
});

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "JwtToken";
        options.Events.OnValidatePrincipal = context =>
        {
            var token = context.Request.Cookies["JwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claims = jwtToken.Claims.ToList();
                var identity = new System.Security.Claims.ClaimsIdentity(claims, "Jwt");

                context.Principal = new System.Security.Claims.ClaimsPrincipal(identity);
            }

            return Task.CompletedTask;
        };
    });

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (!context.Request.Path.HasValue || context.Request.Path == "/")
    {
        if (context.Request.Cookies.ContainsKey("JwtToken"))
        {
            context.Response.Cookies.Delete("JwtToken");
        }
    }
    else
    {
        var token = context.Request.Cookies["JwtToken"];
        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var claims = jwtToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "Jwt", ClaimTypes.Email, ClaimTypes.Role);
            context.User = new ClaimsPrincipal(identity);
        }
    }

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Auth/Login");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();
