using LegajoMinero.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegajoMinero.Api.Infrastructure.Data.Configurations;

public class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> builder)
    {
        builder.ToTable("sesiones");
        builder.HasKey(x => x.IdSesion);

        builder.Property(x => x.IdSesion).HasColumnName("id_sesion");
        builder.Property(x => x.IdUsuario).HasColumnName("id_usuario").IsRequired();
        builder.Property(x => x.RefreshTokenHash).HasColumnName("refresh_token_hash").HasMaxLength(255).IsRequired();
        builder.Property(x => x.IpAcceso).HasColumnName("ip_acceso").HasMaxLength(45);
        builder.Property(x => x.Dispositivo).HasColumnName("dispositivo").HasMaxLength(255);
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.FechaExpiracion).HasColumnName("fecha_expiracion").IsRequired();
        builder.Property(x => x.FechaCierre).HasColumnName("fecha_cierre");
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.Sesiones)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RefreshTokenHash).IsUnique();
        builder.HasIndex(x => new { x.IdUsuario, x.Activa });
    }
}
