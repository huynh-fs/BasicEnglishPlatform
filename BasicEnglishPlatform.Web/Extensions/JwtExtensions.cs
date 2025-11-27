using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BasicEnglishPlatform.Web.Extensions
{
    public static class JwtExtensions
    {
        public static ClaimsPrincipal ToClaimsPrincipal(this string tokenString)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenString);

            var claims = new List<Claim>();

            var id = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == "sub")?.Value;
            if (!string.IsNullOrEmpty(id))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, id));
            }

            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name")?.Value;
            if (!string.IsNullOrEmpty(name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name));
            }

            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}