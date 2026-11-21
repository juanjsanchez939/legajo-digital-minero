using LegajoMinero.Api.Domain.Enums;

namespace LegajoMinero.Api.Domain.Entities;

public class Backup
{
    public int IdBackup { get; set; }
    public DateTime FechaBackup { get; set; } = DateTime.UtcNow;
    public decimal TamanoMb { get; set; }
    public string UbicacionUrl { get; set; } = string.Empty;
    public TipoBackup TipoBackup { get; set; } = TipoBackup.COMPLETO;
    public bool Exitoso { get; set; }
}
