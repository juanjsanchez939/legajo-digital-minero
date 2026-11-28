using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LegajoMinero.Api.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace LegajoMinero.Api.Infrastructure.Security;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAtUtc) Create(Usuario usuario, int idSesion)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta la configuración Jwt:Key.");
        var issuer = _configuration["Jwt:Issuer"] ?? "LegajoMinero";
        var audience = _configuration["Jwt:Audience"] ?? "LegajoMinero";
        var minutes = _configuration.GetValue("Jwt:AccessTokenMinutes", 30);
        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new("sid", idSesion.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
