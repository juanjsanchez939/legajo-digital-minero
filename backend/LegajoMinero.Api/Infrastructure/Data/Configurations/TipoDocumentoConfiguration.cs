using LegajoMinero.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegajoMinero.Api.Infrastructure.Data.Configurations;

public class TipoDocumentoConfiguration : IEntityTypeConfiguration<TipoDocumento>
{
    public void Configure(EntityTypeBuilder<TipoDocumento> builder)
    {
        builder.ToTable("tipos_documento");
        builder.HasKey(x => x.IdTipoDocumento);

        builder.Property(x => x.IdTipoDocumento).HasColumnName("id_tipo_documento");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(255);
        builder.Property(x => x.Obligatorio).HasColumnName("obligatorio").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.DiasAviso).HasColumnName("dias_aviso").HasDefaultValue(15).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
