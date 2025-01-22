using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TaskRegistry.Api.Services
{
    public class JwtService
    {
        private readonly string _key;

        public JwtService(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "La clave JWT no puede ser nula o vacía.");
            }
            _key = key;
        }

        public string GenerateToken(string email, string role, int userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role), 
                new Claim("role", role),
                new Claim("UserId", userId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "TaskRegistry",
                audience: "TaskRegistryUsers",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                    SecurityAlgorithms.HmacSha256
                )
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }

}
