namespace LegajoMinero.Api.Domain.Entities;

public class Sesion
{
    public int IdSesion { get; set; }
    public int IdUsuario { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public string? IpAcceso { get; set; }
    public string? Dispositivo { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
    public DateTime? FechaCierre { get; set; }
    public bool Activa { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
}
