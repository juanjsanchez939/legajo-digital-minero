using LegajoMinero.Api.Domain.Entities;
using LegajoMinero.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegajoMinero.Api.Infrastructure.Data.Configurations;

public class BackupConfiguration : IEntityTypeConfiguration<Backup>
{
    public void Configure(EntityTypeBuilder<Backup> builder)
    {
        builder.ToTable("backups");
        builder.HasKey(x => x.IdBackup);

        builder.Property(x => x.IdBackup).HasColumnName("id_backup");
        builder.Property(x => x.FechaBackup).HasColumnName("fecha_backup").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.TamanoMb).HasColumnName("tamano_mb").HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.UbicacionUrl).HasColumnName("ubicacion_url").HasColumnType("text").IsRequired();
        builder.Property(x => x.TipoBackup)
            .HasColumnName("tipo_backup")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TipoBackup.COMPLETO)
            .IsRequired();
        builder.Property(x => x.Exitoso).HasColumnName("exitoso").IsRequired();
    }
}
