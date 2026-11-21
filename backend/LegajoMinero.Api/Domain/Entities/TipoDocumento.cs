namespace LegajoMinero.Api.Domain.Entities;

public class TipoDocumento
{
    public int IdTipoDocumento { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Obligatorio { get; set; } = true;
    public int DiasAviso { get; set; } = 15;
    public bool Activo { get; set; } = true;

    public ICollection<Documento> Documentos { get; set; } = [];
}
