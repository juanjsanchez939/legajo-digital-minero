using LegajoMinero.Api.Domain.Enums;

namespace LegajoMinero.Api.Domain.Entities;

public class Verificacion
{
    public int IdVerificacion { get; set; }
    public int IdUsuario { get; set; }
    public int? VerificadorUsuarioId { get; set; }
    public int? IdDocumento { get; set; }
    public MetodoVerificacion Metodo { get; set; }
    public string? IpVerificador { get; set; }
    public EstadoVerificado EstadoVerificado { get; set; }
    public DateTime FechaVerificacion { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public Usuario? VerificadorUsuario { get; set; }
    public Documento? Documento { get; set; }
}
