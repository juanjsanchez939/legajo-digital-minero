using LegajoMinero.Api.Domain.Entities;
using LegajoMinero.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegajoMinero.Api.Infrastructure.Data.Configurations;

public class AlertaConfiguration : IEntityTypeConfiguration<Alerta>
{
    public void Configure(EntityTypeBuilder<Alerta> builder)
    {
        builder.ToTable("alertas");
        builder.HasKey(x => x.IdAlerta);

        builder.Property(x => x.IdAlerta).HasColumnName("id_alerta");
        builder.Property(x => x.IdUsuario).HasColumnName("id_usuario").IsRequired();
        builder.Property(x => x.IdDocumento).HasColumnName("id_documento");
        builder.Property(x => x.TipoAlerta)
            .HasColumnName("tipo_alerta")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TipoAlerta.DASHBOARD)
            .IsRequired();
        builder.Property(x => x.Origen)
            .HasColumnName("origen")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(OrigenAlerta.AUTOMATICA)
            .IsRequired();
        builder.Property(x => x.DiasAnticipacion).HasColumnName("dias_anticipacion");
        builder.Property(x => x.Mensaje).HasColumnName("mensaje").HasColumnType("text").IsRequired();
        builder.Property(x => x.FechaEnvio).HasColumnName("fecha_envio").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.FechaLeida).HasColumnName("fecha_leida");
        builder.Property(x => x.Enviada).HasColumnName("enviada").HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.Alertas)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Documento)
            .WithMany(x => x.Alertas)
            .HasForeignKey(x => x.IdDocumento)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.IdUsuario, x.FechaLeida, x.FechaEnvio });
    }
}
