using LegajoMinero.Api.Domain.Enums;

namespace LegajoMinero.Api.Domain.Entities;

public class Documento
{
    public int IdDocumento { get; set; }
    public int IdUsuario { get; set; }
    public int IdTipoDocumento { get; set; }
    public int SubidoPorId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;

    /// <summary>Se persiste cifrado con AES-256.</summary>
    public string? Descripcion { get; set; }

    public DateOnly? FechaVencimiento { get; set; }
    public bool TieneVencimiento { get; set; } = true;
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.VIGENTE;
    public EstadoRevision EstadoRevision { get; set; } = EstadoRevision.PENDIENTE;

    /// <summary>Se persiste cifrado con AES-256.</summary>
    public string? ObservacionRrhh { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public TipoDocumento TipoDocumento { get; set; } = null!;
    public Usuario SubidoPor { get; set; } = null!;
    public ICollection<Alerta> Alertas { get; set; } = [];
    public ICollection<Verificacion> Verificaciones { get; set; } = [];
}
