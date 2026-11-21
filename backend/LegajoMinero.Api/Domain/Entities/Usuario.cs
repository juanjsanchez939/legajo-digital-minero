using LegajoMinero.Api.Domain.Enums;

namespace LegajoMinero.Api.Domain.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Se persiste cifrado con AES-256.</summary>
    public string Telefono { get; set; } = string.Empty;

    public string? Especialidad { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public string ContrasenaHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.TRABAJADOR;
    public Guid QrToken { get; set; } = Guid.NewGuid();
    public bool RecibirAlertasEmail { get; set; } = true;
    public string? TokenRecuperacionHash { get; set; }
    public DateTime? TokenRecuperacionExpira { get; set; }
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAcceso { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime ConsentimientoDatosFecha { get; set; } = DateTime.UtcNow;

    public ICollection<Documento> Documentos { get; set; } = [];
    public ICollection<Documento> DocumentosSubidos { get; set; } = [];
    public ICollection<Alerta> Alertas { get; set; } = [];
    public ICollection<Verificacion> Verificaciones { get; set; } = [];
    public ICollection<Verificacion> VerificacionesRealizadas { get; set; } = [];
    public ICollection<Sesion> Sesiones { get; set; } = [];
}
