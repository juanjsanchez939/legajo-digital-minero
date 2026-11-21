using LegajoMinero.Api.Domain.Enums;

namespace LegajoMinero.Api.Domain.Entities;

public class Alerta
{
    public int IdAlerta { get; set; }
    public int IdUsuario { get; set; }
    public int? IdDocumento { get; set; }
    public TipoAlerta TipoAlerta { get; set; } = TipoAlerta.DASHBOARD;
    public OrigenAlerta Origen { get; set; } = OrigenAlerta.AUTOMATICA;
    public int? DiasAnticipacion { get; set; }

    /// <summary>Se persiste cifrado con AES-256.</summary>
    public string Mensaje { get; set; } = string.Empty;

    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    public DateTime? FechaLeida { get; set; }
    public bool Enviada { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
    public Documento? Documento { get; set; }
}
